using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class BillItem : INotifyPropertyChanged
    {
        private int _quantity;
        private decimal _unitPrice;
        private decimal _totalPrice;
        private decimal? _discountRate;

        [Key]
        public int BillItemId { get; set; }

        public int BillId { get; set; }

        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }

        public int ProductsId { get; set; }

        [ForeignKey("ProductsId")]
        public virtual Products? Product { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                RecalculateTotal();
            }
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice == value) return;
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
                RecalculateTotal();
            }
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (_totalPrice == value) return;
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        /// <summary>
        /// Optional override price per unit. When set, TotalPrice = Quantity × DiscountRate.
        /// Persisted to DB so bill history shows the actual price charged.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountRate
        {
            get => _discountRate;
            set
            {
                if (_discountRate == value) return;
                _discountRate = value;
                OnPropertyChanged(nameof(DiscountRate));
                OnPropertyChanged(nameof(IsDiscountApplied));
                RecalculateTotal();
            }
        }

        [NotMapped]
        public bool IsDiscountApplied => _discountRate.HasValue && _discountRate.Value > 0;

        /// <summary>
        /// Effective price per unit used for display and calculation.
        /// Returns DiscountRate when applied, otherwise UnitPrice.
        /// </summary>
        [NotMapped]
        public decimal EffectiveUnitPrice => IsDiscountApplied ? _discountRate!.Value : _unitPrice;

        public void RecalculateTotal()
        {
            TotalPrice = _quantity * EffectiveUnitPrice;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
