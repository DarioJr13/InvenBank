using AutoMapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;
using InvenBank.API.Services.Interfaces;

namespace InvenBank.API.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(
            ISupplierRepository supplierRepository,
            IMapper mapper,
            ILogger<SupplierService> logger)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ===============================================
        // OBTENER TODOS LOS PROVEEDORES
        // ===============================================

        public async Task<ApiResponse<IEnumerable<SupplierDto>>> GetAllSuppliersAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los proveedores con estadísticas");

                var suppliers = await _supplierRepository.GetSuppliersWithStatsAsync();

                return ApiResponse<IEnumerable<SupplierDto>>.SuccessResult(
                    suppliers,
                    $"Se obtuvieron {suppliers.Count()} proveedores exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los proveedores");
                return ApiResponse<IEnumerable<SupplierDto>>.ErrorResult(
                    "Error al obtener los proveedores"
                );
            }
        }

        // ===============================================
        // OBTENER PROVEEDOR POR ID
        // ===============================================

        public async Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo proveedor por Id: {Id}", id);

                var supplier = await _supplierRepository.GetSupplierWithStatsAsync(id);

                if (supplier == null)
                {
                    _logger.LogWarning("Proveedor no encontrado: {Id}", id);
                    return ApiResponse<SupplierDto>.ErrorResult("Proveedor no encontrado");
                }

                return ApiResponse<SupplierDto>.SuccessResult(
                    supplier,
                    "Proveedor obtenido exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedor por Id: {Id}", id);
                return ApiResponse<SupplierDto>.ErrorResult("Error al obtener el proveedor");
            }
        }

        // ===============================================
        // CREAR NUEVO PROVEEDOR
        // ===============================================

        public async Task<ApiResponse<SupplierDto>> CreateSupplierAsync(CreateSupplierRequest request)
        {
            try
            {
                _logger.LogInformation("Creando nuevo proveedor: {Name}", request.Name);

                // Validar que el nombre no exista
                var nameExists = await _supplierRepository.ExistsByNameAsync(request.Name);
                if (nameExists)
                {
                    return ApiResponse<SupplierDto>.ErrorResult(
                        "Ya existe un proveedor con ese nombre"
                    );
                }

                // Validar que el email no exista (si se proporciona)
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var emailExists = await _supplierRepository.ExistsByEmailAsync(request.Email);
                    if (emailExists)
                    {
                        return ApiResponse<SupplierDto>.ErrorResult(
                            "Ya existe un proveedor con ese email"
                        );
                    }
                }

                // Validar que el TaxId no exista (si se proporciona)
                if (!string.IsNullOrWhiteSpace(request.TaxId))
                {
                    var taxIdExists = await _supplierRepository.ExistsByTaxIdAsync(request.TaxId);
                    if (taxIdExists)
                    {
                        return ApiResponse<SupplierDto>.ErrorResult(
                            "Ya existe un proveedor con ese ID fiscal"
                        );
                    }
                }

                // Mapear y crear el proveedor
                var supplier = _mapper.Map<Supplier>(request);
                var supplierId = await _supplierRepository.CreateAsync(supplier);

                // Obtener el proveedor creado con estadísticas
                var createdSupplier = await _supplierRepository.GetSupplierWithStatsAsync(supplierId);

                _logger.LogInformation("Proveedor creado exitosamente: {Id} - {Name}", supplierId, request.Name);

                return ApiResponse<SupplierDto>.SuccessResult(
                    createdSupplier!,
                    "Proveedor creado exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor: {Name}", request.Name);
                return ApiResponse<SupplierDto>.ErrorResult("Error al crear el proveedor");
            }
        }

        // ===============================================
        // ACTUALIZAR PROVEEDOR
        // ===============================================

        public async Task<ApiResponse<SupplierDto>> UpdateSupplierAsync(int id, UpdateSupplierRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando proveedor Id: {Id}", id);

                // Verificar que el proveedor existe
                var existingSupplier = await _supplierRepository.GetByIdAsync(id);
                if (existingSupplier == null)
                {
                    return ApiResponse<SupplierDto>.ErrorResult("Proveedor no encontrado");
                }

                // Validar que el nombre no esté duplicado
                var nameExists = await _supplierRepository.ExistsByNameAsync(request.Name, id);
                if (nameExists)
                {
                    return ApiResponse<SupplierDto>.ErrorResult(
                        "Ya existe otro proveedor con ese nombre"
                    );
                }

                // Validar que el email no esté duplicado (si se proporciona)
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var emailExists = await _supplierRepository.ExistsByEmailAsync(request.Email, id);
                    if (emailExists)
                    {
                        return ApiResponse<SupplierDto>.ErrorResult(
                            "Ya existe otro proveedor con ese email"
                        );
                    }
                }

                // Validar que el TaxId no esté duplicado (si se proporciona)
                if (!string.IsNullOrWhiteSpace(request.TaxId))
                {
                    var taxIdExists = await _supplierRepository.ExistsByTaxIdAsync(request.TaxId, id);
                    if (taxIdExists)
                    {
                        return ApiResponse<SupplierDto>.ErrorResult(
                            "Ya existe otro proveedor con ese ID fiscal"
                        );
                    }
                }

                // Mapear cambios
                _mapper.Map(request, existingSupplier);
                existingSupplier.Id = id; // Asegurar que mantenga el ID

                // Actualizar
                var success = await _supplierRepository.UpdateAsync(existingSupplier);
                if (!success)
                {
                    return ApiResponse<SupplierDto>.ErrorResult("No se pudo actualizar el proveedor");
                }

                // Obtener el proveedor actualizado
                var updatedSupplier = await _supplierRepository.GetSupplierWithStatsAsync(id);

                _logger.LogInformation("Proveedor actualizado exitosamente: {Id}", id);

                return ApiResponse<SupplierDto>.SuccessResult(
                    updatedSupplier!,
                    "Proveedor actualizado exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proveedor Id: {Id}", id);
                return ApiResponse<SupplierDto>.ErrorResult("Error al actualizar el proveedor");
            }
        }

        // ===============================================
        // ELIMINAR PROVEEDOR (SOFT DELETE)
        // ===============================================

        public async Task<ApiResponse<bool>> DeleteSupplierAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando proveedor Id: {Id}", id);

                // Verificar que el proveedor existe
                var exists = await _supplierRepository.ExistsAsync(id);
                if (!exists)
                {
                    return ApiResponse<bool>.ErrorResult("Proveedor no encontrado");
                }

                // Verificar que no tenga productos asociados
                var hasProducts = await _supplierRepository.HasProductAssociationsAsync(id);
                if (hasProducts)
                {
                    return ApiResponse<bool>.ErrorResult(
                        "No se puede eliminar un proveedor que tiene productos asociados"
                    );
                }

                // Realizar soft delete
                var success = await _supplierRepository.DeleteAsync(id);

                if (success)
                {
                    _logger.LogInformation("Proveedor eliminado exitosamente: {Id}", id);
                    return ApiResponse<bool>.SuccessResult(true, "Proveedor eliminado exitosamente");
                }
                else
                {
                    return ApiResponse<bool>.ErrorResult("No se pudo eliminar el proveedor");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor Id: {Id}", id);
                return ApiResponse<bool>.ErrorResult("Error al eliminar el proveedor");
            }
        }

        // ===============================================
        // OBTENER PROVEEDORES PAGINADOS
        // ===============================================

        public async Task<PagedResponse<SupplierDto>> GetSuppliersPagedAsync(PaginationRequest request)
        {
            try
            {
                _logger.LogInformation("Obteniendo proveedores paginados - Página: {PageNumber}, Tamaño: {PageSize}",
                    request.PageNumber, request.PageSize);

                // Usar el método de paginación del repositorio base
                var pagedResult = await _supplierRepository.GetPagedAsync(request);

                // Convertir las entidades a DTOs
                var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(pagedResult.Data);

                return PagedResponse<SupplierDto>.Create(
                    supplierDtos,
                    request.PageNumber,
                    request.PageSize,
                    pagedResult.TotalRecords
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores paginados");
                return PagedResponse<SupplierDto>.Create(
                    Enumerable.Empty<SupplierDto>(),
                    request.PageNumber,
                    request.PageSize,
                    0
                );
            }
        }

        // ===============================================
        // OBTENER PROVEEDORES POR PRODUCTO
        // ===============================================

        public async Task<ApiResponse<IEnumerable<SupplierDto>>> GetSuppliersByProductAsync(int productId)
        {
            try
            {
                _logger.LogInformation("Obteniendo proveedores para producto: {ProductId}", productId);

                var suppliers = await _supplierRepository.GetActiveSuppliersByProductAsync(productId);
                var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(suppliers);

                return ApiResponse<IEnumerable<SupplierDto>>.SuccessResult(
                    supplierDtos,
                    $"Se obtuvieron {supplierDtos.Count()} proveedores para el producto"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores por producto: {ProductId}", productId);
                return ApiResponse<IEnumerable<SupplierDto>>.ErrorResult(
                    "Error al obtener los proveedores del producto"
                );
            }
        }
    }
}
