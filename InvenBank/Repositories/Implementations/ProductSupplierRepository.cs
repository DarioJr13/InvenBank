namespace InvenBank.API.Repositories.Implementations
{

    using Dapper;
    using global::InvenBank.API.DTOs;
    using global::InvenBank.API.Entities;
    using global::InvenBank.API.Repositories.Interfaces;
    //using InvenBank.API.DTOs;
    //using InvenBank.API.Entities;
    //using InvenBank.API.Repositories.Interfaces;
    using System.Data;

    namespace InvenBank.API.Repositories.Implementations
    {
        public class ProductSupplierRepository : IProductSupplierRepository
        {
            private readonly IDbConnection _connection;
            private readonly ILogger<ProductSupplierRepository> _logger;

            public ProductSupplierRepository(IDbConnection connection, ILogger<ProductSupplierRepository> logger)
            {
                _connection = connection;
                _logger = logger;
            }

            public async Task<IEnumerable<ProductSupplierDto>> GetByProductIdAsync(int productId)
            {
                var sql = @"
                SELECT 
                    ps.Id,
                    ps.ProductId,
                    ps.SupplierId,
                    p.Name AS ProductName,
                    p.SKU AS ProductSKU,
                    s.Name AS SupplierName,
                    ps.Price,
                    ps.Stock,
                    ps.BatchNumber,
                    ps.SupplierSKU,
                    ps.LastRestockDate,
                    ps.IsActive
                FROM ProductSuppliers ps
                INNER JOIN Products p ON ps.ProductId = p.Id
                INNER JOIN Suppliers s ON ps.SupplierId = s.Id
                WHERE ps.ProductId = @ProductId AND ps.IsActive = 1";

                return await _connection.QueryAsync<ProductSupplierDto>(sql, new { ProductId = productId });
            }

            public async Task<int> CreateAsync(ProductSupplier entity)
            {
                try
                {
                    var sql = @"
                INSERT INTO ProductSuppliers (ProductId, SupplierId, Price, Stock, BatchNumber, SupplierSKU, LastRestockDate, IsActive, CreatedAt, UpdatedAt)
                VALUES (@ProductId, @SupplierId, @Price, @Stock, @BatchNumber, @SupplierSKU, @LastRestockDate, 1, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    return await _connection.QuerySingleAsync<int>(sql, entity);
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear ProductSupplier: {Error}", ex.Message);
                    throw;
                }
            }

            public async Task<bool> UpdateAsync(ProductSupplier entity)
            {
                var sql = @"
                UPDATE ProductSuppliers SET
                    Price = @Price,
                    Stock = @Stock,
                    BatchNumber = @BatchNumber,
                    SupplierSKU = @SupplierSKU,
                    LastRestockDate = @LastRestockDate,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id";

                var affectedRows = await _connection.ExecuteAsync(sql, entity);
                return affectedRows > 0;
            }

            public async Task<bool> DeleteAsync(int id)
            {
                var sql = "UPDATE ProductSuppliers SET IsActive = 0, UpdatedAt = GETDATE() WHERE Id = @Id";
                var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id });
                return affectedRows > 0;
            }

            public async Task<ProductSupplier?> GetByIdAsync(int id)
            {
                var sql = "SELECT * FROM ProductSuppliers WHERE Id = @Id AND IsActive = 1";
                return await _connection.QueryFirstOrDefaultAsync<ProductSupplier>(sql, new { Id = id });
            }
        }
    }

}
