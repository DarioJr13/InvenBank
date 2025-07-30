using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.Entities
{
    public class ProductSupplier : ActiveEntity
    {
        public int ProductId { get; set; }
        public int SupplierId { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        [MaxLength(50)]
        public string? SupplierSKU { get; set; }

        public DateTime? LastRestockDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
