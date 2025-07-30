using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.Entities
{
    public class User : ActiveEntity
    {
        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
    }
}
