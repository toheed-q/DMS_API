using DMS.Models;
using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Records customer-level payments. Receivings are NOT allocated to individual
    /// bills — the shop's outstanding is derived as (bills - returns - receivings),
    /// matching the desktop's ledger model.
    /// </summary>
    public class ReceivingService : IReceivingService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ReceivingService> _logger;

        public ReceivingService(
            ApplicationDbContext db,
            ICurrentUserService currentUser,
            ILogger<ReceivingService> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<Result<ReceivingDto>> CreateAsync(CreateReceivingRequest request)
        {
            if (request.Amount <= 0)
                return Result<ReceivingDto>.Failure("Amount must be greater than zero.", ErrorType.Validation);

            var shop = await _db.Shops
                .AsNoTracking()
                .Where(s => s.ShopId == request.ShopId)
                .Select(s => new { s.ShopId, s.Name })
                .FirstOrDefaultAsync();

            if (shop is null)
                return Result<ReceivingDto>.Failure($"Shop {request.ShopId} was not found.", ErrorType.NotFound);

            var receiving = new LedgerReceiving
            {
                ShopId = request.ShopId,
                Amount = request.Amount,
                ReceivedDate = request.ReceivedDate,
                Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks.Trim(),
                CreatedByUserId = _currentUser.UserId,
                CreatedByName = _currentUser.Username,
                CreatedAt = DateTime.Now
            };

            _db.LedgerReceivings.Add(receiving);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Receiving {ReceivingId} of {Amount} recorded for shop {ShopId} by {User}",
                receiving.LedgerReceivingId, receiving.Amount, receiving.ShopId, _currentUser.Username);

            return Result<ReceivingDto>.Success(new ReceivingDto
            {
                Id = receiving.LedgerReceivingId,
                ShopId = receiving.ShopId,
                ShopName = shop.Name,
                Amount = receiving.Amount,
                ReceivedDate = receiving.ReceivedDate,
                Remarks = receiving.Remarks,
                CreatedByName = receiving.CreatedByName,
                CreatedAt = receiving.CreatedAt
            });
        }

        public async Task<Result<PagedResult<ReceivingDto>>> GetByShopAsync(int shopId, PaginationQuery query)
        {
            var shopExists = await _db.Shops.AsNoTracking().AnyAsync(s => s.ShopId == shopId);
            if (!shopExists)
                return Result<PagedResult<ReceivingDto>>.Failure($"Shop {shopId} was not found.", ErrorType.NotFound);

            var receivings = _db.LedgerReceivings.AsNoTracking().Where(r => r.ShopId == shopId);

            var totalCount = await receivings.CountAsync();

            var items = await receivings
                .OrderByDescending(r => r.ReceivedDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => new ReceivingDto
                {
                    Id = r.LedgerReceivingId,
                    ShopId = r.ShopId,
                    ShopName = r.Shop.Name,
                    Amount = r.Amount,
                    ReceivedDate = r.ReceivedDate,
                    Remarks = r.Remarks,
                    CreatedByName = r.CreatedByName,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Result<PagedResult<ReceivingDto>>.Success(new PagedResult<ReceivingDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }
    }
}
