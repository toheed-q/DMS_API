using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>Categories are reference data (used for dropdowns/filters), returned in full.</summary>
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;

        public CategoryService(ApplicationDbContext db) => _db = db;

        public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync()
        {
            var categories = await _db.Categories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.CategoryId,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    CompanyName = c.CompanyName,
                    ProductCount = c.Products.Count
                })
                .ToListAsync();

            return Result<IReadOnlyList<CategoryDto>>.Success(categories);
        }

        public async Task<Result<CategoryDto>> GetByIdAsync(int id)
        {
            var category = await _db.Categories.AsNoTracking()
                .Where(c => c.CategoryId == id)
                .Select(c => new CategoryDto
                {
                    Id = c.CategoryId,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    CompanyName = c.CompanyName,
                    ProductCount = c.Products.Count
                })
                .FirstOrDefaultAsync();

            return category is null
                ? Result<CategoryDto>.Failure($"Category {id} was not found.", ErrorType.NotFound)
                : Result<CategoryDto>.Success(category);
        }
    }
}
