namespace InvenBank.API.DTOs
{
    public class RelatedProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal? MinPrice { get; set; }
        public bool IsAvailable { get; set; }
    }

}
