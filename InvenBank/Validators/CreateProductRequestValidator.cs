using FluentValidation;
using InvenBank.API.DTOs.Requests;

namespace InvenBank.API.Validators
{
    // ===============================================
    // VALIDADOR PARA CREAR PRODUCTO
    // ===============================================

    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del producto es requerido")
                .Length(2, 200).WithMessage("El nombre debe tener entre 2 y 200 caracteres")
                .Matches("^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\\s\\-&().]+$")
                .WithMessage("El nombre solo puede contener letras, números, espacios y caracteres especiales básicos");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("El SKU es requerido")
                .Length(3, 50).WithMessage("El SKU debe tener entre 3 y 50 caracteres")
                .Matches("^[A-Z0-9\\-]+$")
                .WithMessage("El SKU solo puede contener letras mayúsculas, números y guiones");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).WithMessage("La URL de la imagen no es válida")
                .MaximumLength(500).WithMessage("La URL de la imagen no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Brand)
                .MaximumLength(100).WithMessage("La marca no puede exceder 100 caracteres")
                .Matches("^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\\s\\-&.]+$")
                .WithMessage("La marca solo puede contener letras, números, espacios y caracteres básicos")
                .When(x => !string.IsNullOrEmpty(x.Brand));

            RuleFor(x => x.Specifications)
                .Must(BeValidJsonOrEmpty).WithMessage("Las especificaciones deben estar en formato JSON válido")
                .When(x => !string.IsNullOrEmpty(x.Specifications));
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidJsonOrEmpty(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return true;

            try
            {
                System.Text.Json.JsonDocument.Parse(json);
                return true;
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }
    }

}