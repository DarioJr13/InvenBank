using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;

namespace InvenBank.API.Repositories.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
        Task<PagedResponse<CategoryDto>> GetCategoriesPagedAsync(PaginationRequest request);
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoryHierarchyAsync();
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesByParentAsync(int? parentId);
    }
}
