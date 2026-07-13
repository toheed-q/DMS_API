using DMS.Models;
using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Bill creation. Everything is computed server-side (money is never trusted
    /// from the client) and written in ONE transaction: bill + items + stock
    /// deduction + ledger entry + upfront receiving. All-or-nothing.
    /// </summary>
    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _db;
        private readonly ISystemSettingsService _settings;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<BillService> _logger;

        public BillService(
            ApplicationDbContext db,
            ISystemSettingsService settings,
            ICurrentUserService currentUser,
            ILogger<BillService> logger)
        {
            _db = db;
            _settings = settings;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<Result<BillDto>> CreateAsync(CreateBillRequest request)
        {
            if (request.Items.Count == 0)
                return Fail("A bill must contain at least one item.", ErrorType.Validation);

            if (request.PaidAmount < 0)
                return Fail("Paid amount cannot be negative.", ErrorType.Validation);

            // ---- Customer: shop sale or walk-in ----
            var isWalkIn = !request.ShopId.HasValue;
            Shop? shop = null;

            if (!isWalkIn)
            {
                shop = await _db.Shops.FirstOrDefaultAsync(s => s.ShopId == request.ShopId!.Value);
                if (shop is null)
                    return Fail($"Shop {request.ShopId} was not found.", ErrorType.NotFound);
            }
            else if (string.IsNullOrWhiteSpace(request.WalkInCustomerName))
            {
                return Fail("A walk-in sale requires a customer name (or provide a shopId).", ErrorType.Validation);
            }

            // SECURITY: a field salesman may only bill as themselves — whatever
            // SalesmanId they send is ignored and forced to their own.
            if (_currentUser.IsSalesman)
            {
                if (_currentUser.SalesmanId is null)
                    return Fail("Your login is not linked to a salesman record.", ErrorType.Forbidden);

                request.SalesmanId = _currentUser.SalesmanId;
            }

            if (request.SalesmanId.HasValue &&
                !await _db.Salesmen.AnyAsync(s => s.SalesmanId == request.SalesmanId.Value))
            {
                return Fail($"Salesman {request.SalesmanId} was not found.", ErrorType.NotFound);
            }

            // ---- Load every referenced product in ONE query (no N+1) ----
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.ProductsId))
                .ToDictionaryAsync(p => p.ProductsId);

            var missing = productIds.Where(id => !products.ContainsKey(id)).ToList();
            if (missing.Count > 0)
                return Fail($"Product(s) not found: {string.Join(", ", missing)}.", ErrorType.NotFound);

            // ---- Stock check (aggregate per product: same item may appear twice) ----
            foreach (var group in request.Items.GroupBy(i => i.ProductId))
            {
                var product = products[group.Key];
                var required = group.Sum(i => i.Quantity);
                var available = product.AvailableStock ?? 0;
                if (available < required)
                    return Fail(
                        $"Insufficient stock for {product.Name}. Available: {available}, required: {required}.",
                        ErrorType.Validation);
            }

            // ---- Money: computed server-side ----
            var taxes = await _settings.GetTaxSettingsAsync();
            var billDate = request.BillDate ?? DateTime.Now;

            var lines = new List<(CreateBillItemRequest Req, Products Product, decimal UnitPrice, decimal TotalPrice)>();
            decimal subtotal = 0m;

            foreach (var item in request.Items)
            {
                var product = products[item.ProductId];
                var unitPrice = item.UnitPrice ?? product.TradePrice ?? product.CompanyPrice;
                if (unitPrice < 0)
                    return Fail($"Unit price for {product.Name} cannot be negative.", ErrorType.Validation);

                // A DiscountRate (> 0) is an override price per unit.
                var effectiveUnitPrice = item.DiscountRate is > 0 ? item.DiscountRate.Value : unitPrice;
                var totalPrice = item.Quantity * effectiveUnitPrice;

                subtotal += totalPrice;
                lines.Add((item, product, unitPrice, totalPrice));
            }

            var discountAmount = request.IsPercentageDiscount
                ? subtotal * request.Discount / 100m
                : request.Discount;

            if (discountAmount < 0)
                return Fail("Discount cannot be negative.", ErrorType.Validation);
            if (discountAmount > subtotal)
                return Fail("Discount cannot exceed the subtotal.", ErrorType.Validation);

            var afterDiscount = subtotal - discountAmount;

            var gst      = request.ApplyGST        ? afterDiscount * taxes.GstPercent        / 100m : 0m;
            var advTax   = request.ApplyAdvanceTax ? afterDiscount * taxes.AdvanceTaxPercent / 100m : 0m;
            var salesTax = request.ApplySalesTax   ? afterDiscount * taxes.SalesTaxPercent   / 100m : 0m;
            var otherTax = request.ApplyOtherTax   ? afterDiscount * taxes.OtherTaxPercent   / 100m : 0m;

            // This bill's own total — what gets posted to the ledger.
            var billTotal = afterDiscount + gst + advTax + salesTax + otherTax;

            var previousDues = 0m;
            if (!isWalkIn && request.AddPreviousDuesToBill)
                previousDues = await GetOutstandingAsync(shop!.ShopId);

            var totalAmount = billTotal + previousDues;

            // ---- Persist atomically ----
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var bill = new Bill
                {
                    ShopId = shop?.ShopId,
                    SMId = request.SalesmanId,
                    BillDate = billDate,
                    Subtotal = subtotal,
                    Discount = discountAmount,
                    ApplyGST = request.ApplyGST,
                    ApplyAdvanceTax = request.ApplyAdvanceTax,
                    ApplySalesTax = request.ApplySalesTax,
                    ApplyOtherTax = request.ApplyOtherTax,
                    GSTAmount = gst,
                    AdvanceTaxAmount = advTax,
                    SalesTaxAmount = salesTax,
                    OtherTaxAmount = otherTax,
                    PreviousDues = previousDues,
                    TotalAmount = totalAmount,
                    PaidAmount = request.PaidAmount,
                    WalkInCustomerName = isWalkIn ? request.WalkInCustomerName : null,
                    WalkInCustomerPhone = isWalkIn ? request.WalkInCustomerPhone : null,
                    CreatedByUserId = _currentUser.UserId
                };

                _db.Bills.Add(bill);
                await _db.SaveChangesAsync();   // assigns BillId

                foreach (var line in lines)
                {
                    _db.BillItems.Add(new BillItem
                    {
                        BillId = bill.BillId,
                        ProductsId = line.Product.ProductsId,
                        ProductName = line.Product.Name,
                        Quantity = line.Req.Quantity,
                        UnitPrice = line.UnitPrice,
                        DiscountRate = line.Req.DiscountRate,
                        TotalPrice = line.TotalPrice
                    });

                    // Reuse the already-tracked product — no extra query.
                    line.Product.AvailableStock = (line.Product.AvailableStock ?? 0) - line.Req.Quantity;
                }

                if (!isWalkIn && shop is not null)
                {
                    _db.LedgerEntries.Add(new LedgerEntry
                    {
                        ShopId = shop.ShopId,
                        BillId = bill.BillId,
                        BillTotal = billTotal,
                        PaidAmount = request.PaidAmount,
                        RemainingAmount = billTotal - request.PaidAmount,
                        EntryDate = billDate
                    });

                    // Receivings are customer-level, so anything paid at the counter is
                    // recorded as a receiving (this is what counts toward Total Received).
                    if (request.PaidAmount > 0)
                    {
                        _db.LedgerReceivings.Add(new LedgerReceiving
                        {
                            ShopId = shop.ShopId,
                            Amount = request.PaidAmount,
                            ReceivedDate = billDate,
                            Remarks = $"Paid at sale (Bill #{bill.BillId})",
                            CreatedByUserId = _currentUser.UserId,
                            CreatedByName = _currentUser.Username,
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Bill {BillId} created for {Customer} — total {BillTotal}, paid {Paid}, by {User}",
                    bill.BillId,
                    shop?.Name ?? request.WalkInCustomerName,
                    billTotal, request.PaidAmount, _currentUser.Username);

                var created = await GetByIdAsync(bill.BillId);
                return created;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create bill — rolled back");
                throw;   // global handler turns this into a clean 500
            }
        }

        public async Task<Result<BillDto>> GetByIdAsync(int id)
        {
            var bill = await _db.Bills
                .AsNoTracking()
                .Where(b => b.BillId == id)
                .Select(b => new BillDto
                {
                    Id = b.BillId,
                    ShopId = b.ShopId,
                    ShopName = b.Shop != null ? b.Shop.Name : null,
                    SalesmanId = b.SMId,
                    SalesmanName = b.Salesman != null ? b.Salesman.FullName : null,
                    WalkInCustomerName = b.WalkInCustomerName,
                    WalkInCustomerPhone = b.WalkInCustomerPhone,
                    BillDate = b.BillDate,
                    Subtotal = b.Subtotal,
                    Discount = b.Discount,
                    GSTAmount = b.GSTAmount,
                    AdvanceTaxAmount = b.AdvanceTaxAmount,
                    SalesTaxAmount = b.SalesTaxAmount,
                    OtherTaxAmount = b.OtherTaxAmount,
                    PreviousDues = b.PreviousDues,
                    TotalAmount = b.TotalAmount,
                    BillTotal = b.TotalAmount - b.PreviousDues,
                    PaidAmount = b.PaidAmount,
                    RemainingAmount = (b.TotalAmount - b.PreviousDues) - b.PaidAmount,
                    Items = b.BillItems.Select(i => new BillItemDto
                    {
                        ProductId = i.ProductsId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountRate = i.DiscountRate,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return bill is null
                ? Fail($"Bill {id} was not found.", ErrorType.NotFound)
                : Result<BillDto>.Success(bill);
        }

        public async Task<Result<BillsPagedResult>> GetBillsAsync(BillQuery query)
        {
            // SECURITY: a field salesman only ever sees their OWN bills, no matter
            // what filter they pass.
            if (_currentUser.IsSalesman)
            {
                if (_currentUser.SalesmanId is null)
                    return Result<BillsPagedResult>.Failure(
                        "Your login is not linked to a salesman record.", ErrorType.Forbidden);

                query.SalesmanId = _currentUser.SalesmanId;
            }

            var bills = _db.Bills.AsNoTracking();

            if (query.From.HasValue)
                bills = bills.Where(b => b.BillDate >= query.From.Value.Date);

            if (query.To.HasValue)
                bills = bills.Where(b => b.BillDate < query.To.Value.Date.AddDays(1));

            if (query.ShopId.HasValue)
                bills = bills.Where(b => b.ShopId == query.ShopId.Value);

            if (query.SalesmanId.HasValue)
                bills = bills.Where(b => b.SMId == query.SalesmanId.Value);

            // "Paid" = what was collected at the counter covers this bill's own total
            // (previous dues rolled into the bill are excluded from that comparison).
            if (query.PaidOnly == true)
                bills = bills.Where(b => b.PaidAmount >= b.TotalAmount - b.PreviousDues);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                int.TryParse(term, out var billNo);

                bills = bills.Where(b =>
                    (billNo > 0 && b.BillId == billNo) ||
                    (b.Shop != null && b.Shop.Name.Contains(term)) ||
                    (b.WalkInCustomerName != null && b.WalkInCustomerName.Contains(term)) ||
                    b.BillItems.Any(bi => bi.ProductName.Contains(term)));
            }

            var totalCount = await bills.CountAsync();
            var totalBilled = await bills.SumAsync(b => (decimal?)(b.TotalAmount - b.PreviousDues)) ?? 0m;
            var totalPaid = await bills.SumAsync(b => (decimal?)b.PaidAmount) ?? 0m;

            var items = await bills
                .OrderByDescending(b => b.BillDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BillSummaryDto
                {
                    Id = b.BillId,
                    BillDate = b.BillDate,
                    ShopId = b.ShopId,
                    CustomerName = b.Shop != null ? b.Shop.Name : b.WalkInCustomerName,
                    IsWalkIn = b.ShopId == null,
                    SalesmanId = b.SMId,
                    SalesmanName = b.Salesman != null ? b.Salesman.FullName : null,
                    ItemCount = b.BillItems.Count,
                    BillTotal = b.TotalAmount - b.PreviousDues,
                    PaidAmount = b.PaidAmount,
                    RemainingAmount = (b.TotalAmount - b.PreviousDues) - b.PaidAmount,
                    IsPaid = b.PaidAmount >= b.TotalAmount - b.PreviousDues
                })
                .ToListAsync();

            return Result<BillsPagedResult>.Success(new BillsPagedResult
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalBilled = totalBilled,
                TotalPaid = totalPaid
            });
        }

        /// <summary>Shop's current outstanding: bills - returns - receivings (never negative).</summary>
        private async Task<decimal> GetOutstandingAsync(int shopId)
        {
            var ledger = _db.LedgerEntries.AsNoTracking().Where(l => l.ShopId == shopId);

            var billed   = await ledger.SumAsync(l => (decimal?)l.BillTotal) ?? 0m;
            var returned = await ledger.SumAsync(l => (decimal?)l.ReturnAmount) ?? 0m;
            var received = await _db.LedgerReceivings.AsNoTracking()
                .Where(r => r.ShopId == shopId)
                .SumAsync(r => (decimal?)r.Amount) ?? 0m;

            var outstanding = billed - returned - received;
            return outstanding > 0m ? outstanding : 0m;
        }

        private static Result<BillDto> Fail(string message, ErrorType type)
            => Result<BillDto>.Failure(message, type);
    }
}
