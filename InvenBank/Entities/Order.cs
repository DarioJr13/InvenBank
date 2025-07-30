using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.Entities
{
    public class Order : BaseEntity
    {
        [Required, MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        public int UserId { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Delivered, Cancelled

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveryDate { get; set; }

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        public string? Notes { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
