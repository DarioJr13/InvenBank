using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.DTOs.Requests
{
    public class CreateSupplierRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [MaxLength(50)]
        public string? TaxId { get; set; }
    }
}
