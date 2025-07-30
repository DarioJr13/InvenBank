using AutoMapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;
using InvenBank.API.Services.Interfaces;

namespace InvenBank.API.Services.Implementations
{
    public class ProductSupplierService : IProductSupplierService
    {
        private readonly IProductSupplierRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductSupplierService> _logger;

        public ProductSupplierService(
            IProductSupplierRepository repository,
            IMapper mapper,
            ILogger<ProductSupplierService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<ProductSupplierDto>>> GetByProductIdAsync(int productId)
        {
            var result = await _repository.GetByProductIdAsync(productId);
            return ApiResponse<IEnumerable<ProductSupplierDto>>.SuccessResult(result, "Proveedores del producto cargados correctamente");
        }

        public async Task<ApiResponse<ProductSupplierDto>> CreateAsync(CreateProductSupplierRequest request)
        {
            var entity = _mapper.Map<ProductSupplier>(request);
            var id = await _repository.CreateAsync(entity);
            var inserted = await _repository.GetByIdAsync(id);
            var dto = _mapper.Map<ProductSupplierDto>(inserted);
            return ApiResponse<ProductSupplierDto>.SuccessResult(dto, "Asociación creada");
        }

        public async Task<ApiResponse<ProductSupplierDto>> UpdateAsync(int id, UpdateProductSupplierRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<ProductSupplierDto>.ErrorResult("Asociación no encontrada");

            _mapper.Map(request, existing);
            var success = await _repository.UpdateAsync(existing);
            if (!success)
                return ApiResponse<ProductSupplierDto>.ErrorResult("No se pudo actualizar");

            var updated = await _repository.GetByIdAsync(id);
            var dto = _mapper.Map<ProductSupplierDto>(updated);
            return ApiResponse<ProductSupplierDto>.SuccessResult(dto, "Asociación actualizada");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var success = await _repository.DeleteAsync(id);
            return success
                ? ApiResponse<bool>.SuccessResult(true, "Asociación eliminada")
                : ApiResponse<bool>.ErrorResult("No se pudo eliminar");
        }
    }
}
