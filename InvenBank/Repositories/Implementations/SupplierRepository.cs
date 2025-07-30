using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;
using System.Data;

namespace InvenBank.API.Repositories.Implementations
{
    public class SupplierRepository : BaseRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(IDbConnection connection, ILogger<SupplierRepository> logger)
            : base(connection, logger, "Suppliers", "Id")
        {
        }

        // ===============================================
        // CRUD ESPECÍFICO PARA PROVEEDORES
        // ===============================================

        public override async Task<int> CreateAsync(Supplier entity)
        {
            try
            {
                var sql = @"
                INSERT INTO Suppliers (Name, ContactPerson, Email, Phone, Address, TaxId, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Name, @ContactPerson, @Email, @Phone, @Address, @TaxId, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS int);";

                _logger.LogInformation("Creando nuevo proveedor: {Name}", entity.Name);

                var id = await _connection.QuerySingleAsync<int>(sql, entity);

                _logger.LogInformation("Proveedor creado exitosamente con Id: {Id}", id);

                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor: {Name}", entity.Name);
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(Supplier entity)
        {
            try
            {
                var sql = @"
                UPDATE Suppliers 
                SET Name = @Name, 
                    ContactPerson = @ContactPerson,
                    Email = @Email,
                    Phone = @Phone,
                    Address = @Address,
                    TaxId = @TaxId,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id";

                _logger.LogInformation("Actualizando proveedor Id: {Id}", entity.Id);

                var affectedRows = await _connection.ExecuteAsync(sql, entity);

                var success = affectedRows > 0;
                _logger.LogInformation("Proveedor actualizado: {Success}", success);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proveedor Id: {Id}", entity.Id);
                throw;
            }
        }

        // ===============================================
        // CONSULTAS CON ESTADÍSTICAS (USANDO VIEWS)
        // ===============================================

        public async Task<IEnumerable<SupplierDto>> GetSuppliersWithStatsAsync()
        {
            try
            {
                var sql = @"
                SELECT 
                    s.Id,
                    s.Name,
                    s.ContactPerson,
                    s.Email,
                    s.Phone,
                    s.Address,
                    s.TaxId,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt,
                    ISNULL(stats.ProductCount, 0) AS ProductCount,
                    ISNULL(stats.TotalInventoryValue, 0) AS TotalInventoryValue,
                    ISNULL(stats.TotalSales, 0) AS TotalSales,
                    ISNULL(stats.TotalRevenue, 0) AS TotalRevenue,
                    stats.LastUpdate
                FROM Suppliers s
                LEFT JOIN vw_SupplierStats stats ON s.Id = stats.SupplierId
                WHERE s.IsActive = 1
                ORDER BY s.Name";

                _logger.LogInformation("Obteniendo proveedores con estadísticas");

                var suppliers = await _connection.QueryAsync<SupplierDto>(sql);

                _logger.LogInformation("Obtenidos {Count} proveedores con estadísticas", suppliers.Count());

                return suppliers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores con estadísticas");
                throw;
            }
        }

        public async Task<SupplierDto?> GetSupplierWithStatsAsync(int id)
        {
            try
            {
                var sql = @"
                SELECT 
                    s.Id,
                    s.Name,
                    s.ContactPerson,
                    s.Email,
                    s.Phone,
                    s.Address,
                    s.TaxId,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt,
                    ISNULL(stats.ProductCount, 0) AS ProductCount,
                    ISNULL(stats.TotalInventoryValue, 0) AS TotalInventoryValue,
                    ISNULL(stats.TotalSales, 0) AS TotalSales,
                    ISNULL(stats.TotalRevenue, 0) AS TotalRevenue,
                    stats.LastUpdate
                FROM Suppliers s
                LEFT JOIN vw_SupplierStats stats ON s.Id = stats.SupplierId
                WHERE s.Id = @Id";

                _logger.LogInformation("Obteniendo proveedor con estadísticas Id: {Id}", id);

                var supplier = await _connection.QueryFirstOrDefaultAsync<SupplierDto>(sql, new { Id = id });

                if (supplier != null)
                {
                    _logger.LogInformation("Proveedor encontrado: {Name}", supplier.Name);
                }
                else
                {
                    _logger.LogWarning("Proveedor no encontrado Id: {Id}", id);
                }

                return supplier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedor con estadísticas Id: {Id}", id);
                throw;
            }
        }

        // ===============================================
        // CONSULTAS DE RELACIONES
        // ===============================================

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersByProductAsync(int productId)
        {
            try
            {
                var sql = @"
               SELECT DISTINCT s.Id,
                   s.Name,
                   s.ContactPerson,
                   s.Email,
                   s.Phone,
                   CAST(s.Address AS varchar(MAX)) AS Address,
                   s.TaxId,
                   s.IsActive,
                   s.CreatedAt,
                   s.UpdatedAt
                FROM Suppliers s
                INNER JOIN ProductSuppliers ps ON s.Id = ps.SupplierId
                WHERE ps.ProductId = 4 
                  AND ps.IsActive = 1 
                  AND s.IsActive = 1
                ORDER BY s.Name";

                _logger.LogInformation("Obteniendo proveedores activos para producto: {ProductId}", productId);

                var suppliers = await _connection.QueryAsync<Supplier>(sql, new { ProductId = productId });

                _logger.LogInformation("Obtenidos {Count} proveedores para producto {ProductId}", suppliers.Count(), productId);

                return suppliers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores por producto: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> HasProductAssociationsAsync(int supplierId)
        {
            try
            {
                var sql = "SELECT COUNT(1) FROM ProductSuppliers WHERE SupplierId = @SupplierId AND IsActive = 1";

                var count = await _connection.QuerySingleAsync<int>(sql, new { SupplierId = supplierId });

                _logger.LogInformation("Proveedor {SupplierId} tiene {Count} asociaciones de productos", supplierId, count);

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar asociaciones de productos para proveedor Id: {SupplierId}", supplierId);
                throw;
            }
        }

        // ===============================================
        // VALIDACIONES DE UNICIDAD
        // ===============================================

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            try
            {
                var sql = @"
                SELECT COUNT(1) FROM Suppliers 
                WHERE LOWER(Name) = LOWER(@Name) 
                  AND IsActive = 1
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

                var count = await _connection.QuerySingleAsync<int>(sql, new { Name = name, ExcludeId = excludeId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de proveedor por nombre: {Name}", name);
                throw;
            }
        }

        public async Task<bool> ExistsByTaxIdAsync(string taxId, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxId))
                    return false;

                var sql = @"
                SELECT COUNT(1) FROM Suppliers 
                WHERE LOWER(TaxId) = LOWER(@TaxId) 
                  AND IsActive = 1
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

                var count = await _connection.QuerySingleAsync<int>(sql, new { TaxId = taxId, ExcludeId = excludeId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de proveedor por TaxId: {TaxId}", taxId);
                throw;
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                var sql = @"
                SELECT COUNT(1) FROM Suppliers 
                WHERE LOWER(Email) = LOWER(@Email) 
                  AND IsActive = 1
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

                var count = await _connection.QuerySingleAsync<int>(sql, new { Email = email, ExcludeId = excludeId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de proveedor por Email: {Email}", email);
                throw;
            }
        }

        // ===============================================
        // OVERRIDE SEARCH PARA PROVEEDORES
        // ===============================================

        protected override string GetWhereClause(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return "WHERE IsActive = 1";

            return @"WHERE IsActive = 1 
                 AND (Name LIKE @SearchTerm 
                      OR ContactPerson LIKE @SearchTerm
                      OR Email LIKE @SearchTerm
                      OR TaxId LIKE @SearchTerm)";
        }
    }
}
