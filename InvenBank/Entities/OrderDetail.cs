using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int OrderId { get; set; }
        public int ProductSupplierId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        // Computed property
        public decimal TotalPrice => Quantity * UnitPrice;

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual ProductSupplier ProductSupplier { get; set; } = null!;
    }
}
