using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;

namespace InvenBank.API.Services.Interfaces
{
    public interface IProductSupplierService
    {
        Task<ApiResponse<IEnumerable<ProductSupplierDto>>> GetByProductIdAsync(int productId);
        Task<ApiResponse<ProductSupplierDto>> CreateAsync(CreateProductSupplierRequest request);
        Task<ApiResponse<ProductSupplierDto>> UpdateAsync(int id, UpdateProductSupplierRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

}
