using InvenBank.API.DTOs;
using InvenBank.API.Entities;

namespace InvenBank.API.Repositories.Interfaces
{
    public interface IProductSupplierRepository
    {
        Task<IEnumerable<ProductSupplierDto>> GetByProductIdAsync(int productId);
        Task<int> CreateAsync(ProductSupplier entity);
        Task<bool> UpdateAsync(ProductSupplier entity);
        Task<bool> DeleteAsync(int id);
        Task<ProductSupplier?> GetByIdAsync(int id);
    }

}
