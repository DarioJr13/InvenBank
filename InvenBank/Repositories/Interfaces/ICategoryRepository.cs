using InvenBank.API.DTOs;
using InvenBank.API.Entities;

namespace InvenBank.API.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesWithStatsAsync();
        Task<CategoryDto?> GetCategoryWithStatsAsync(int id);
        Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync();
        Task<IEnumerable<Category>> GetCategoriesByParentAsync(int? parentId);
        Task<bool> HasChildCategoriesAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    }
}
