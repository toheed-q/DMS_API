using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        [Required]
        public string? Name { get; set; } = "";
        public int? ParentCategoryId { get; set; }  // Nullable for root categories
        public string? CompanyName { get; set; }  // Optional company name
        
        // Navigation properties
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Products> Products { get; set; } = new List<Products>();
        
        // Helper property to get category hierarchy path
        public string? FullPath => ParentCategory != null 
            ? $"{ParentCategory.FullPath} > {Name}" 
            : Name;
    }
}
