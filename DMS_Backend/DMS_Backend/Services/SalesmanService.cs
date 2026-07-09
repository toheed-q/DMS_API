using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>Salesman reads: paginated, searchable, optionally active-only.</summary>
    public class SalesmanService : ISalesmanService
    {
        private readonly ApplicationDbContext _db;

        public SalesmanService(ApplicationDbContext db) => _db = db;

        public async Task<Result<PagedResult<SalesmanDto>>> GetSalesmenAsync(PaginationQuery query, bool? activeOnly)
        {
            var salesmen = _db.Salesmen.AsNoTracking();

            if (activeOnly == true)
                salesmen = salesmen.Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                salesmen = salesmen.Where(s =>
                    s.FullName.Contains(term) ||
                    s.PhoneNumber.Contains(term) ||
                    (s.CNIC != null && s.CNIC.Contains(term)));
            }

            var totalCount = await salesmen.CountAsync();

            var items = await salesmen
                .OrderBy(s => s.FullName)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new SalesmanDto
                {
                    Id = s.SalesmanId,
                    FullName = s.FullName,
                    PhoneNumber = s.PhoneNumber,
                    CNIC = s.CNIC,
                    MonthlySalary = s.MonthlySalary,
                    IncentiveType = s.IncentiveType,
                    IncentiveValue = s.IncentiveValue,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Result<PagedResult<SalesmanDto>>.Success(new PagedResult<SalesmanDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<Result<SalesmanDto>> GetByIdAsync(int id)
        {
            var salesman = await _db.Salesmen.AsNoTracking()
                .Where(s => s.SalesmanId == id)
                .Select(s => new SalesmanDto
                {
                    Id = s.SalesmanId,
                    FullName = s.FullName,
                    PhoneNumber = s.PhoneNumber,
                    CNIC = s.CNIC,
                    MonthlySalary = s.MonthlySalary,
                    IncentiveType = s.IncentiveType,
                    IncentiveValue = s.IncentiveValue,
                    IsActive = s.IsActive
                })
                .FirstOrDefaultAsync();

            return salesman is null
                ? Result<SalesmanDto>.Failure($"Salesman {id} was not found.", ErrorType.NotFound)
                : Result<SalesmanDto>.Success(salesman);
        }
    }
}
