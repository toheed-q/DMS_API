using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{
    public class SystemSetting
    {
        [Key]
        public int SettingId { get; set; }
        
        [Required]
        public string SettingKey { get; set; } = "";
        
        [Required]
        public string SettingName { get; set; } = "";
        
        [Required]
        public string SettingValue { get; set; } = "";
        
        public string? Description { get; set; }
    }
}
