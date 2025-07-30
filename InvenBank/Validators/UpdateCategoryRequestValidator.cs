using FluentValidation;
using InvenBank.API.DTOs.Requests;

namespace InvenBank.API.Validators
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la categoría es requerido")
                .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres")
                .Matches("^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\\s\\-&]+$")
                .WithMessage("El nombre solo puede contener letras, números, espacios, guiones y &");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ParentId)
                .GreaterThan(0).WithMessage("El ID de la categoría padre debe ser mayor a 0")
                .When(x => x.ParentId.HasValue);

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("El estado activo/inactivo es requerido");
        }
    }

}
