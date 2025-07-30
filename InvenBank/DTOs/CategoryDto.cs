namespace InvenBank.API.DTOs
{
    public class CategoryDto : ActiveDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto> Children { get; set; } = new();
    }
}
