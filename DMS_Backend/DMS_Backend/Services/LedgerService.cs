using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Customer ledger. Receivings are customer-level (never allocated to a single
    /// bill), so the balance is derived:
    ///     NetBills    = Bills - Returns
    ///     Outstanding = max(0, NetBills - Received)
    ///     Excess      = max(0, Received - NetBills)   (informational only)
    /// </summary>
    public class LedgerService : ILedgerService
    {
        private readonly ApplicationDbContext _db;

        public LedgerService(ApplicationDbContext db) => _db = db;

        public async Task<Result<LedgerSummaryDto>> GetSummaryAsync(int shopId)
        {
            var shop = await _db.Shops.AsNoTracking()
                .Where(s => s.ShopId == shopId)
                .Select(s => new { s.ShopId, s.Name })
                .FirstOrDefaultAsync();

            if (shop is null)
                return Result<LedgerSummaryDto>.Failure($"Shop {shopId} was not found.", ErrorType.NotFound);

            var ledger = _db.LedgerEntries.AsNoTracking().Where(l => l.ShopId == shopId);

            // Aggregated entirely in SQL — rows are never loaded.
            var totalBills = await ledger.SumAsync(l => (decimal?)l.BillTotal) ?? 0m;
            var totalReturns = await ledger.SumAsync(l => (decimal?)l.ReturnAmount) ?? 0m;
            var totalReceived = await _db.LedgerReceivings.AsNoTracking()
                .Where(r => r.ShopId == shopId)
                .SumAsync(r => (decimal?)r.Amount) ?? 0m;

            var netBills = totalBills - totalReturns;

            return Result<LedgerSummaryDto>.Success(new LedgerSummaryDto
            {
                ShopId = shop.ShopId,
                ShopName = shop.Name,
                TotalBills = totalBills,
                TotalReturns = totalReturns,
                NetBills = netBills,
                TotalReceived = totalReceived,
                Outstanding = Math.Max(0m, netBills - totalReceived),
                Excess = Math.Max(0m, totalReceived - netBills)
            });
        }

        public async Task<Result<PagedResult<LedgerEntryDto>>> GetEntriesAsync(int shopId, LedgerEntryQuery query)
        {
            var shopExists = await _db.Shops.AsNoTracking().AnyAsync(s => s.ShopId == shopId);
            if (!shopExists)
                return Result<PagedResult<LedgerEntryDto>>.Failure($"Shop {shopId} was not found.", ErrorType.NotFound);

            var entries = _db.LedgerEntries.AsNoTracking().Where(l => l.ShopId == shopId);

            if (query.From.HasValue)
                entries = entries.Where(l => l.EntryDate >= query.From.Value.Date);

            if (query.To.HasValue)
                entries = entries.Where(l => l.EntryDate < query.To.Value.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                int.TryParse(term, out var billNo);

                entries = entries.Where(l =>
                    (l.ManualItems != null && l.ManualItems.Contains(term)) ||
                    (l.Bill != null && l.Bill.BillItems.Any(bi => bi.ProductName.Contains(term))) ||
                    (billNo > 0 && l.BillId == billNo));
            }

            var totalCount = await entries.CountAsync();

            // Project only what's needed; bill item names come back as a small list
            // and are formatted in memory (page size is bounded, so this is cheap).
            var rows = await entries
                .OrderByDescending(l => l.EntryDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(l => new
                {
                    l.LedgerEntryId,
                    l.EntryDate,
                    l.BillId,
                    l.BillTotal,
                    l.ReturnAmount,
                    l.IsReturn,
                    l.IsManualEntry,
                    l.ManualItems,
                    l.ReturnProducts,
                    SalesmanName = l.Salesman != null ? l.Salesman.FullName : null,
                    BillItems = l.Bill == null
                        ? null
                        : l.Bill.BillItems.Select(bi => new { bi.ProductName, bi.Quantity }).ToList()
                })
                .ToListAsync();

            var items = rows.Select(r => new LedgerEntryDto
            {
                Id = r.LedgerEntryId,
                EntryDate = r.EntryDate,
                BillId = r.BillId,
                Items = r.IsManualEntry
                    ? r.ManualItems
                    : r.BillItems is { Count: > 0 }
                        ? string.Join(", ", r.BillItems.Select(bi => $"{bi.ProductName}({bi.Quantity})"))
                        : null,
                BillTotal = r.BillTotal,
                ReturnAmount = r.ReturnAmount,
                IsReturn = r.IsReturn,
                IsManualEntry = r.IsManualEntry,
                ReturnProducts = r.ReturnProducts,
                SalesmanName = r.SalesmanName
            }).ToList();

            return Result<PagedResult<LedgerEntryDto>>.Success(new PagedResult<LedgerEntryDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }
    }
}
