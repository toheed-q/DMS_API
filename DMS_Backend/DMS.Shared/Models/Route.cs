using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation property for related Shops
        public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
    }
}
