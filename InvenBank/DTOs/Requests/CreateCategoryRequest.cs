using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.DTOs.Requests
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentId { get; set; }
    }
}
