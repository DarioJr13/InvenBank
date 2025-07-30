using FluentValidation;
using InvenBank.API.DTOs.Requests;

namespace InvenBank.API.Validators
{
    public class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
    {
        public CreateSupplierRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del proveedor es requerido")
                .Length(2, 200).WithMessage("El nombre debe tener entre 2 y 200 caracteres");

            RuleFor(x => x.ContactPerson)
                .MaximumLength(150).WithMessage("El contacto no puede exceder 150 caracteres")
                .When(x => !string.IsNullOrEmpty(x.ContactPerson));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del email es inválido")
                .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[\d\s\-\(\)]{7,20}$")
                .WithMessage("El formato del teléfono es inválido")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.TaxId)
                .MaximumLength(50).WithMessage("El ID fiscal no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.TaxId));
        }
    }
}
