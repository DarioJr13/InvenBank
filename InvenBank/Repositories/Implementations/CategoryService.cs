using AutoMapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;

namespace InvenBank.API.Repositories.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ===============================================
        // OBTENER TODAS LAS CATEGORÍAS
        // ===============================================

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las categorías con estadísticas");

                var categories = await _categoryRepository.GetCategoriesWithStatsAsync();

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(
                    categories,
                    $"Se obtuvieron {categories.Count()} categorías exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las categorías");
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                    "Error al obtener las categorías"
                );
            }
        }

        // ===============================================
        // OBTENER CATEGORÍA POR ID
        // ===============================================

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo categoría por Id: {Id}", id);

                var category = await _categoryRepository.GetCategoryWithStatsAsync(id);

                if (category == null)
                {
                    _logger.LogWarning("Categoría no encontrada: {Id}", id);
                    return ApiResponse<CategoryDto>.ErrorResult("Categoría no encontrada");
                }

                return ApiResponse<CategoryDto>.SuccessResult(
                    category,
                    "Categoría obtenida exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría por Id: {Id}", id);
                return ApiResponse<CategoryDto>.ErrorResult("Error al obtener la categoría");
            }
        }

        // ===============================================
        // CREAR NUEVA CATEGORÍA
        // ===============================================

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                _logger.LogInformation("Creando nueva categoría: {Name}", request.Name);

                // Validar que el nombre no exista
                var nameExists = await _categoryRepository.ExistsByNameAsync(request.Name);
                if (nameExists)
                {
                    return ApiResponse<CategoryDto>.ErrorResult(
                        "Ya existe una categoría con ese nombre"
                    );
                }

                // Validar que la categoría padre exista (si se especifica)
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(request.ParentId.Value);
                    if (!parentExists)
                    {
                        return ApiResponse<CategoryDto>.ErrorResult(
                            "La categoría padre especificada no existe"
                        );
                    }
                }

                // Mapear y crear la categoría
                var category = _mapper.Map<Category>(request);
                var categoryId = await _categoryRepository.CreateAsync(category);

                // Obtener la categoría creada con estadísticas
                var createdCategory = await _categoryRepository.GetCategoryWithStatsAsync(categoryId);

                _logger.LogInformation("Categoría creada exitosamente: {Id} - {Name}", categoryId, request.Name);

                return ApiResponse<CategoryDto>.SuccessResult(
                    createdCategory!,
                    "Categoría creada exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría: {Name}", request.Name);
                return ApiResponse<CategoryDto>.ErrorResult("Error al crear la categoría");
            }
        }

        // ===============================================
        // ACTUALIZAR CATEGORÍA
        // ===============================================

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando categoría Id: {Id}", id);

                // Verificar que la categoría existe
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResult("Categoría no encontrada");
                }

                // Validar que el nombre no esté duplicado
                var nameExists = await _categoryRepository.ExistsByNameAsync(request.Name, id);
                if (nameExists)
                {
                    return ApiResponse<CategoryDto>.ErrorResult(
                        "Ya existe otra categoría con ese nombre"
                    );
                }

                // Validar que la categoría padre exista (si se especifica)
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(request.ParentId.Value);
                    if (!parentExists)
                    {
                        return ApiResponse<CategoryDto>.ErrorResult(
                            "La categoría padre especificada no existe"
                        );
                    }

                    // Evitar ciclos: una categoría no puede ser padre de sí misma
                    if (request.ParentId.Value == id)
                    {
                        return ApiResponse<CategoryDto>.ErrorResult(
                            "Una categoría no puede ser padre de sí misma"
                        );
                    }
                }

                // Mapear cambios
                _mapper.Map(request, existingCategory);
                existingCategory.Id = id; // Asegurar que mantenga el ID

                // Actualizar
                var success = await _categoryRepository.UpdateAsync(existingCategory);
                if (!success)
                {
                    return ApiResponse<CategoryDto>.ErrorResult("No se pudo actualizar la categoría");
                }

                // Obtener la categoría actualizada
                var updatedCategory = await _categoryRepository.GetCategoryWithStatsAsync(id);

                _logger.LogInformation("Categoría actualizada exitosamente: {Id}", id);

                return ApiResponse<CategoryDto>.SuccessResult(
                    updatedCategory!,
                    "Categoría actualizada exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría Id: {Id}", id);
                return ApiResponse<CategoryDto>.ErrorResult("Error al actualizar la categoría");
            }
        }

        // ===============================================
        // ELIMINAR CATEGORÍA (SOFT DELETE)
        // ===============================================

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando categoría Id: {Id}", id);

                // Verificar que la categoría existe
                var exists = await _categoryRepository.ExistsAsync(id);
                if (!exists)
                {
                    return ApiResponse<bool>.ErrorResult("Categoría no encontrada");
                }

                // Verificar que no tenga categorías hijas
                var hasChildren = await _categoryRepository.HasChildCategoriesAsync(id);
                if (hasChildren)
                {
                    return ApiResponse<bool>.ErrorResult(
                        "No se puede eliminar una categoría que tiene subcategorías"
                    );
                }

                // Verificar que no tenga productos asociados
                var hasProducts = await _categoryRepository.HasProductsAsync(id);
                if (hasProducts)
                {
                    return ApiResponse<bool>.ErrorResult(
                        "No se puede eliminar una categoría que tiene productos asociados"
                    );
                }

                // Realizar soft delete
                var success = await _categoryRepository.DeleteAsync(id);

                if (success)
                {
                    _logger.LogInformation("Categoría eliminada exitosamente: {Id}", id);
                    return ApiResponse<bool>.SuccessResult(true, "Categoría eliminada exitosamente");
                }
                else
                {
                    return ApiResponse<bool>.ErrorResult("No se pudo eliminar la categoría");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría Id: {Id}", id);
                return ApiResponse<bool>.ErrorResult("Error al eliminar la categoría");
            }
        }

        // ===============================================
        // OBTENER CATEGORÍAS PAGINADAS
        // ===============================================

        public async Task<PagedResponse<CategoryDto>> GetCategoriesPagedAsync(PaginationRequest request)
        {
            try
            {
                _logger.LogInformation("Obteniendo categorías paginadas - Página: {PageNumber}, Tamaño: {PageSize}",
                    request.PageNumber, request.PageSize);

                // Usar el método de paginación del repositorio base
                var pagedResult = await _categoryRepository.GetPagedAsync(request);

                // Convertir las entidades a DTOs
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(pagedResult.Data);

                return PagedResponse<CategoryDto>.Create(
                    categoryDtos,
                    request.PageNumber,
                    request.PageSize,
                    pagedResult.TotalRecords
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías paginadas");
                return PagedResponse<CategoryDto>.Create(
                    Enumerable.Empty<CategoryDto>(),
                    request.PageNumber,
                    request.PageSize,
                    0
                );
            }
        }

        // ===============================================
        // OBTENER JERARQUÍA DE CATEGORÍAS
        // ===============================================

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoryHierarchyAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo jerarquía de categorías");

                var hierarchy = await _categoryRepository.GetCategoryHierarchyAsync();

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(
                    hierarchy,
                    "Jerarquía de categorías obtenida exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener jerarquía de categorías");
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                    "Error al obtener la jerarquía de categorías"
                );
            }
        }

        // ===============================================
        // OBTENER CATEGORÍAS POR PADRE
        // ===============================================

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesByParentAsync(int? parentId)
        {
            try
            {
                _logger.LogInformation("Obteniendo categorías por padre: {ParentId}", parentId?.ToString() ?? "NULL");

                var categories = await _categoryRepository.GetCategoriesByParentAsync(parentId);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(
                    categoryDtos,
                    $"Se obtuvieron {categoryDtos.Count()} categorías exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías por padre: {ParentId}", parentId);
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResult(
                    "Error al obtener las categorías"
                );
            }
        }
    }
}
