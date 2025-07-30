using InvenBank.API.DTOs;
using InvenBank.API.Entities;

namespace InvenBank.API.Repositories.Interfaces
{
    public interface ISupplierRepository : IBaseRepository<Supplier>
    {
        Task<IEnumerable<SupplierDto>> GetSuppliersWithStatsAsync();
        Task<SupplierDto?> GetSupplierWithStatsAsync(int id);
        Task<IEnumerable<Supplier>> GetActiveSuppliersByProductAsync(int productId);
        Task<bool> HasProductAssociationsAsync(int supplierId);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<bool> ExistsByTaxIdAsync(string taxId, int? excludeId = null);
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    }

}
