using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class DailySalesReport
    {
        [Key]
        public int DSRId { get; set; }

        [Required]
        public int SalesmanId { get; set; }

        [ForeignKey("SalesmanId")]
        public virtual Salesman Salesman { get; set; } = null!;

        [Required]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        public int? RouteId { get; set; }

        [ForeignKey("RouteId")]
        public virtual Route? Route { get; set; }

        public string? VehicleNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSales { get; set; } = 0;

        public int ShopsVisited { get; set; } = 0;

        public string? Notes { get; set; }

        // Financial Summary Fields
        public bool IsDiscountPercentage { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ArrearCreditReceived { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TodayCreditSale { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalExpenses { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CashReceived { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<DSRItem> DSRItems { get; set; } = new List<DSRItem>();

        public virtual ICollection<DSRExpense> DSRExpenses { get; set; } = new List<DSRExpense>();
    }
}
