using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;

namespace InvenBank.API.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<ApiResponse<IEnumerable<SupplierDto>>> GetAllSuppliersAsync();
        Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(int id);
        Task<ApiResponse<SupplierDto>> CreateSupplierAsync(CreateSupplierRequest request);
        Task<ApiResponse<SupplierDto>> UpdateSupplierAsync(int id, UpdateSupplierRequest request);
        Task<ApiResponse<bool>> DeleteSupplierAsync(int id);
        Task<PagedResponse<SupplierDto>> GetSuppliersPagedAsync(PaginationRequest request);
        Task<ApiResponse<IEnumerable<SupplierDto>>> GetSuppliersByProductAsync(int productId);
    }

}
