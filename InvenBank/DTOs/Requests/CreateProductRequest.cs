using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.DTOs.Requests
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El SKU es requerido")]
        [MaxLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
        public int CategoryId { get; set; }

        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
        public string? ImageUrl { get; set; }

        [MaxLength(100, ErrorMessage = "La marca no puede exceder 100 caracteres")]
        public string? Brand { get; set; }

        public string? Specifications { get; set; } // JSON format - validar en el servicio
    }
}
