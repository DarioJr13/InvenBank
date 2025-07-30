using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.Entities
{
    public class Product : ActiveEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required, MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        public string? Specifications { get; set; } // JSON format

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}
