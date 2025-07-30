namespace InvenBank.API.Entities
{
    public class Wishlist : BaseEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
