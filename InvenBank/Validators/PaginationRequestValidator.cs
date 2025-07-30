using FluentValidation;
using InvenBank.API.DTOs.Requests;

namespace InvenBank.API.Validators
{
    public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
    {
        public PaginationRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(200).WithMessage("El término de búsqueda no puede exceder 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.SortBy)
                .MaximumLength(50).WithMessage("El campo de ordenamiento no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SortBy));
        }
    }
}
