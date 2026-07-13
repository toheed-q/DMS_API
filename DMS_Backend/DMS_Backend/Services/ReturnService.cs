using DMS.Models;
using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Customer returns. A return is a ledger entry (IsReturn) that reduces the
    /// shop's outstanding AND is attributed to a salesman. Any returned products
    /// go back into stock — all in one transaction.
    /// </summary>
    public class ReturnService : IReturnService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ReturnService> _logger;

        public ReturnService(
            ApplicationDbContext db,
            ICurrentUserService currentUser,
            ILogger<ReturnService> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<Result<ReturnDto>> CreateAsync(CreateReturnRequest request)
        {
            if (request.ReturnAmount <= 0)
                return Fail("Return amount must be greater than zero.", ErrorType.Validation);

            var shop = await _db.Shops.AsNoTracking()
                .Where(s => s.ShopId == request.ShopId)
                .Select(s => new { s.ShopId, s.Name })
                .FirstOrDefaultAsync();
            if (shop is null)
                return Fail($"Shop {request.ShopId} was not found.", ErrorType.NotFound);

            // SECURITY: a field salesman may only file returns against themselves —
            // whatever SalesmanId they send is ignored and forced to their own.
            if (_currentUser.IsSalesman)
            {
                if (_currentUser.SalesmanId is null)
                    return Fail("Your login is not linked to a salesman record.", ErrorType.Forbidden);

                request.SalesmanId = _currentUser.SalesmanId.Value;
            }

            var salesman = await _db.Salesmen.AsNoTracking()
                .Where(s => s.SalesmanId == request.SalesmanId)
                .Select(s => new { s.SalesmanId, s.FullName })
                .FirstOrDefaultAsync();
            if (salesman is null)
                return Fail($"Salesman {request.SalesmanId} was not found.", ErrorType.NotFound);

            // ---- Load returned products (if any) in ONE query ----
            var products = new Dictionary<int, Products>();
            if (request.Items.Count > 0)
            {
                var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
                products = await _db.Products
                    .Where(p => productIds.Contains(p.ProductsId))
                    .ToDictionaryAsync(p => p.ProductsId);

                var missing = productIds.Where(id => !products.ContainsKey(id)).ToList();
                if (missing.Count > 0)
                    return Fail($"Product(s) not found: {string.Join(", ", missing)}.", ErrorType.NotFound);
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Restock returned products and capture a readable snapshot
                // ("Pepsi 1.5L(12), Juice(6)") so history stays clear even if a
                // product is later renamed or removed.
                var summary = new List<string>();
                foreach (var group in request.Items.GroupBy(i => i.ProductId))
                {
                    var product = products[group.Key];
                    var quantity = group.Sum(i => i.Quantity);

                    product.AvailableStock = (product.AvailableStock ?? 0) + quantity;
                    summary.Add($"{product.Name}({quantity})");
                }

                var entry = new LedgerEntry
                {
                    ShopId = request.ShopId,
                    BillId = null,
                    BillTotal = 0,
                    PaidAmount = 0,
                    RemainingAmount = 0,
                    EntryDate = request.ReturnDate,
                    IsManualEntry = true,
                    IsReturn = true,
                    ReturnAmount = request.ReturnAmount,
                    SalesmanId = request.SalesmanId,
                    ManualItems = request.Description.Trim(),
                    ReturnProducts = summary.Count > 0 ? string.Join(", ", summary) : null
                };

                _db.LedgerEntries.Add(entry);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Return {EntryId} of {Amount} recorded for shop {ShopId}, salesman {SalesmanId}, by {User}",
                    entry.LedgerEntryId, entry.ReturnAmount, entry.ShopId, entry.SalesmanId, _currentUser.Username);

                return Result<ReturnDto>.Success(new ReturnDto
                {
                    Id = entry.LedgerEntryId,
                    ShopId = entry.ShopId,
                    ShopName = shop.Name,
                    SalesmanId = entry.SalesmanId,
                    SalesmanName = salesman.FullName,
                    ReturnAmount = entry.ReturnAmount,
                    ReturnDate = entry.EntryDate,
                    Description = entry.ManualItems,
                    ReturnProducts = entry.ReturnProducts
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to record return — rolled back");
                throw;
            }
        }

        public async Task<Result<ReturnsPagedResult>> GetReturnsAsync(ReturnQuery query)
        {
            // SECURITY: a field salesman only ever sees their OWN returns.
            if (_currentUser.IsSalesman)
            {
                if (_currentUser.SalesmanId is null)
                    return Result<ReturnsPagedResult>.Failure(
                        "Your login is not linked to a salesman record.", ErrorType.Forbidden);

                query.SalesmanId = _currentUser.SalesmanId;
            }

            var returns = _db.LedgerEntries.AsNoTracking().Where(l => l.IsReturn);

            if (query.SalesmanId.HasValue)
                returns = returns.Where(l => l.SalesmanId == query.SalesmanId.Value);

            if (query.ShopId.HasValue)
                returns = returns.Where(l => l.ShopId == query.ShopId.Value);

            if (query.From.HasValue)
                returns = returns.Where(l => l.EntryDate >= query.From.Value.Date);

            if (query.To.HasValue)
                returns = returns.Where(l => l.EntryDate < query.To.Value.Date.AddDays(1));

            var totalCount = await returns.CountAsync();
            var totalReturned = await returns.SumAsync(l => (decimal?)l.ReturnAmount) ?? 0m;

            var items = await returns
                .OrderByDescending(l => l.EntryDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(l => new ReturnDto
                {
                    Id = l.LedgerEntryId,
                    ShopId = l.ShopId,
                    ShopName = l.Shop.Name,
                    SalesmanId = l.SalesmanId,
                    SalesmanName = l.Salesman != null ? l.Salesman.FullName : null,
                    ReturnAmount = l.ReturnAmount,
                    ReturnDate = l.EntryDate,
                    Description = l.ManualItems,
                    ReturnProducts = l.ReturnProducts
                })
                .ToListAsync();

            return Result<ReturnsPagedResult>.Success(new ReturnsPagedResult
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalReturned = totalReturned
            });
        }

        private static Result<ReturnDto> Fail(string message, ErrorType type)
            => Result<ReturnDto>.Failure(message, type);
    }
}
