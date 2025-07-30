namespace InvenBank.API.DTOs
{
    public class WishlistDto : BaseDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string? ProductBrand { get; set; }
        public DateTime AddedDate { get; set; }
        public string? Notes { get; set; }
    }

}
