using FluentValidation;
using InvenBank.API.DTOs.Requests;

namespace InvenBank.API.Validators
{
    // ===============================================
    // VALIDADOR PARA BÚSQUEDA DE PRODUCTOS
    // ===============================================

    public class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
    {
        public ProductSearchRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(200).WithMessage("El término de búsqueda no puede exceder 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("El ID de categoría debe ser mayor a 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.Brand)
                .MaximumLength(100).WithMessage("La marca no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Brand));

            RuleFor(x => x.MinPrice)
                .GreaterThan(0).WithMessage("El precio mínimo debe ser mayor a 0")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThan(0).WithMessage("El precio máximo debe ser mayor a 0")
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x)
                .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice.Value <= x.MaxPrice.Value)
                .WithMessage("El precio mínimo no puede ser mayor al precio máximo")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            RuleFor(x => x.SortBy)
                .MaximumLength(50).WithMessage("El campo de ordenamiento no puede exceder 50 caracteres")
                .Must(BeValidSortField).WithMessage("Campo de ordenamiento no válido")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.StockStatus)
                .Must(BeValidStockStatus).WithMessage("Estado de stock no válido")
                .When(x => !string.IsNullOrEmpty(x.StockStatus));
        }

        private bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
                return true;

            var validSortFields = new[] { "Name", "SKU", "Brand", "CreatedAt", "MinPrice", "MaxPrice", "TotalStock", "Category" };
            return validSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeValidStockStatus(string? stockStatus)
        {
            if (string.IsNullOrEmpty(stockStatus))
                return true;

            var validStatuses = new[] { "Disponible", "Stock Bajo", "Sin Stock" };
            return validStatuses.Contains(stockStatus, StringComparer.OrdinalIgnoreCase);
        }
    }
}
