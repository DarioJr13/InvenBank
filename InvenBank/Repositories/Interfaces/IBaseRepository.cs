using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;

namespace InvenBank.API.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<int> CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<PagedResponse<T>> GetPagedAsync(PaginationRequest request);
    }

}
