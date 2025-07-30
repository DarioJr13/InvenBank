using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;
using System.Data;
using System.Text.Json;

namespace InvenBank.API.Repositories.Implementations
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(IDbConnection connection, ILogger<ProductRepository> logger)
            : base(connection, logger, "Products", "Id")
        {
        }

        // ===============================================
        // CRUD ESPECÍFICO PARA PRODUCTOS
        // ===============================================

        public override async Task<int> CreateAsync(Product entity)
        {
            try
            {
                var sql = @"
                INSERT INTO Products (Name, Description, SKU, CategoryId, ImageUrl, Brand, Specifications, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Name, @Description, @SKU, @CategoryId, @ImageUrl, @Brand, @Specifications, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS int);";

                _logger.LogInformation("Creando nuevo producto: {Name} - SKU: {SKU}", entity.Name, entity.SKU);

                var id = await _connection.QuerySingleAsync<int>(sql, entity);

                _logger.LogInformation("Producto creado exitosamente con Id: {Id}", id);

                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto: {Name} - SKU: {SKU}", entity.Name, entity.SKU);
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(Product entity)
        {
            try
            {
                var sql = @"
                UPDATE Products 
                SET Name = @Name, 
                    Description = @Description,
                    SKU = @SKU,
                    CategoryId = @CategoryId,
                    ImageUrl = @ImageUrl,
                    Brand = @Brand,
                    Specifications = @Specifications,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id";

                _logger.LogInformation("Actualizando producto Id: {Id} - SKU: {SKU}", entity.Id, entity.SKU);

                var affectedRows = await _connection.ExecuteAsync(sql, entity);

                var success = affectedRows > 0;
                _logger.LogInformation("Producto actualizado: {Success}", success);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto Id: {Id}", entity.Id);
                throw;
            }
        }

        // ===============================================
        // CONSULTAS CON ESTADÍSTICAS COMPLETAS
        // ===============================================

        public async Task<IEnumerable<ProductDto>> GetProductsWithStatsAsync()
        {
            try
            {
                var sql = @"
                SELECT 
                    p.Id, p.Name, p.Description, p.SKU, p.CategoryId, p.ImageUrl, p.Brand, p.Specifications,
                    p.IsActive, p.CreatedAt, p.UpdatedAt,
                    c.Name AS CategoryName,
                    ISNULL(pc.CategoryPath, c.Name) AS CategoryPath,
                    
                    -- Estadísticas de precio y stock
                    ISNULL(pricing.MinPrice, 0) AS MinPrice,
                    ISNULL(pricing.MaxPrice, 0) AS MaxPrice,
                    ISNULL(pricing.BestPrice, 0) AS BestPrice,
                    ISNULL(pricing.TotalStock, 0) AS TotalStock,
                    ISNULL(pricing.SupplierCount, 0) AS SupplierCount,
                    ISNULL(pricing.LastRestockDate, NULL) AS LastRestockDate,
                    
                    -- Estadísticas de ventas
                    ISNULL(sales.TotalSold, 0) AS TotalSold,
                    ISNULL(sales.TotalRevenue, 0) AS TotalRevenue,
                    ISNULL(wishlist.WishlistCount, 0) AS WishlistCount,
                    
                    -- Estado calculado
                    CASE 
                        WHEN ISNULL(pricing.TotalStock, 0) = 0 THEN 'Sin Stock'
                        WHEN ISNULL(pricing.TotalStock, 0) <= 5 THEN 'Stock Bajo'
                        ELSE 'Disponible'
                    END AS StockStatus,
                    
                    CASE WHEN ISNULL(pricing.TotalStock, 0) > 0 THEN 1 ELSE 0 END AS IsAvailable
                    
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN (
                    SELECT 
                        CategoryId,
                        STUFF((SELECT ' > ' + Name 
                              FROM Categories c2 
                              WHERE c2.ParentId = c1.Id 
                              FOR XML PATH('')), 1, 3, '') AS CategoryPath
                    FROM Categories c1
                    WHERE ParentId IS NULL
                ) pc ON c.Id = pc.CategoryId OR c.ParentId = pc.CategoryId
                LEFT JOIN (
                    SELECT 
                        ProductId,
                        MIN(Price) AS MinPrice,
                        MAX(Price) AS MaxPrice,
                        MIN(Price) AS BestPrice,
                        SUM(Stock) AS TotalStock,
                        COUNT(*) AS SupplierCount,
                        MAX(LastRestockDate) AS LastRestockDate
                    FROM ProductSuppliers 
                    WHERE IsActive = 1
                    GROUP BY ProductId
                ) pricing ON p.Id = pricing.ProductId
                LEFT JOIN (
                    SELECT 
                        ps.ProductId,
                        SUM(od.Quantity) AS TotalSold,
                        SUM(od.TotalPrice) AS TotalRevenue
                    FROM OrderDetails od
                    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
                    INNER JOIN Orders o ON od.OrderId = o.Id
                    WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
                    GROUP BY ps.ProductId
                ) sales ON p.Id = sales.ProductId
                LEFT JOIN (
                    SELECT ProductId, COUNT(*) AS WishlistCount
                    FROM Wishlists
                    GROUP BY ProductId
                ) wishlist ON p.Id = wishlist.ProductId
                WHERE p.IsActive = 1
                ORDER BY p.Name";

                _logger.LogInformation("Obteniendo productos con estadísticas completas");

                var products = await _connection.QueryAsync<ProductDto>(sql);

                _logger.LogInformation("Obtenidos {Count} productos con estadísticas", products.Count());

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con estadísticas");
                throw;
            }
        }

        public async Task<ProductDto?> GetProductWithStatsAsync(int id)
        {
            try
            {
                var sql = @"
                SELECT 
                    p.Id, p.Name, p.Description, p.SKU, p.CategoryId, p.ImageUrl, p.Brand, p.Specifications,
                    p.IsActive, p.CreatedAt, p.UpdatedAt,
                    c.Name AS CategoryName,
                    ISNULL(pc.CategoryPath, c.Name) AS CategoryPath,
                    
                    -- Estadísticas de precio y stock
                    ISNULL(pricing.MinPrice, 0) AS MinPrice,
                    ISNULL(pricing.MaxPrice, 0) AS MaxPrice,
                    ISNULL(pricing.BestPrice, 0) AS BestPrice,
                    ISNULL(pricing.TotalStock, 0) AS TotalStock,
                    ISNULL(pricing.SupplierCount, 0) AS SupplierCount,
                    ISNULL(pricing.LastRestockDate, NULL) AS LastRestockDate,
                    
                    -- Estadísticas de ventas
                    ISNULL(sales.TotalSold, 0) AS TotalSold,
                    ISNULL(sales.TotalRevenue, 0) AS TotalRevenue,
                    ISNULL(wishlist.WishlistCount, 0) AS WishlistCount,
                    
                    -- Estado calculado
                    CASE 
                        WHEN ISNULL(pricing.TotalStock, 0) = 0 THEN 'Sin Stock'
                        WHEN ISNULL(pricing.TotalStock, 0) <= 5 THEN 'Stock Bajo'
                        ELSE 'Disponible'
                    END AS StockStatus,
                    
                    CASE WHEN ISNULL(pricing.TotalStock, 0) > 0 THEN 1 ELSE 0 END AS IsAvailable
                    
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN (
                    SELECT 
                        CategoryId,
                        STUFF((SELECT ' > ' + Name 
                              FROM Categories c2 
                              WHERE c2.ParentId = c1.Id 
                              FOR XML PATH('')), 1, 3, '') AS CategoryPath
                    FROM Categories c1
                    WHERE ParentId IS NULL
                ) pc ON c.Id = pc.CategoryId OR c.ParentId = pc.CategoryId
                LEFT JOIN (
                    SELECT 
                        ProductId,
                        MIN(Price) AS MinPrice,
                        MAX(Price) AS MaxPrice,
                        MIN(Price) AS BestPrice,
                        SUM(Stock) AS TotalStock,
                        COUNT(*) AS SupplierCount,
                        MAX(LastRestockDate) AS LastRestockDate
                    FROM ProductSuppliers 
                    WHERE IsActive = 1
                    GROUP BY ProductId
                ) pricing ON p.Id = pricing.ProductId
                LEFT JOIN (
                    SELECT 
                        ps.ProductId,
                        SUM(od.Quantity) AS TotalSold,
                        SUM(od.TotalPrice) AS TotalRevenue
                    FROM OrderDetails od
                    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
                    INNER JOIN Orders o ON od.OrderId = o.Id
                    WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
                    GROUP BY ps.ProductId
                ) sales ON p.Id = sales.ProductId
                LEFT JOIN (
                    SELECT ProductId, COUNT(*) AS WishlistCount
                    FROM Wishlists
                    GROUP BY ProductId
                ) wishlist ON p.Id = wishlist.ProductId
                WHERE p.Id = @Id";

                _logger.LogInformation("Obteniendo producto con estadísticas Id: {Id}", id);

                var product = await _connection.QueryFirstOrDefaultAsync<ProductDto>(sql, new { Id = id });

                if (product != null)
                {
                    _logger.LogInformation("Producto encontrado: {Name} - SKU: {SKU}", product.Name, product.SKU);
                }
                else
                {
                    _logger.LogWarning("Producto no encontrado Id: {Id}", id);
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto con estadísticas Id: {Id}", id);
                throw;
            }
        }

        // ===============================================
        // CONSULTA DETALLADA CON PROVEEDORES
        // ===============================================

        public async Task<ProductDetailDto?> GetProductDetailAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo detalle completo del producto Id: {Id}", id);

                // Consulta principal del producto
                var productSql = @"
                SELECT 
                    p.Id, p.Name, p.Description, p.SKU, p.CategoryId, p.ImageUrl, p.Brand, p.Specifications,
                    p.IsActive, p.CreatedAt, p.UpdatedAt,
                    c.Name AS CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Id = @Id";

                var product = await _connection.QueryFirstOrDefaultAsync<ProductDetailDto>(productSql, new { Id = id });

                if (product == null)
                    return null;

                // Consulta de proveedores
                var suppliersSql = @"
                SELECT 
                    ps.Id AS ProductSupplierId,
                    ps.SupplierId,
                    s.Name AS SupplierName,
                    ps.Price,
                    ps.Stock,
                    ps.BatchNumber,
                    ps.SupplierSKU,
                    ps.LastRestockDate,
                    ps.IsActive
                FROM ProductSuppliers ps
                INNER JOIN Suppliers s ON ps.SupplierId = s.Id
                WHERE ps.ProductId = @Id AND ps.IsActive = 1
                ORDER BY ps.Price ASC";

                product.Suppliers = (await _connection.QueryAsync<ProductSupplierInfo>(suppliersSql, new { Id = id })).ToList();

                // Parsear especificaciones JSON si existen
                if (!string.IsNullOrEmpty(product.Specifications))
                {
                    try
                    {
                        var specs = JsonSerializer.Deserialize<Dictionary<string, object>>(product.Specifications);
                        product.ParsedSpecifications = specs?.Select(kvp => new ProductSpecificationItem
                        {
                            Key = kvp.Key,
                            Value = kvp.Value?.ToString() ?? "",
                            Type = DetermineSpecificationType(kvp.Value)
                        }).ToList() ?? new List<ProductSpecificationItem>();
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Error al parsear especificaciones JSON para producto {Id}", id);
                        product.ParsedSpecifications = new List<ProductSpecificationItem>();
                    }
                }

                // Productos relacionados (misma categoría)
                var relatedSql = @"
                SELECT TOP 5
                    p.Id, p.Name, p.SKU, p.ImageUrl,
                    MIN(ps.Price) AS MinPrice,
                    CASE WHEN SUM(ps.Stock) > 0 THEN 1 ELSE 0 END AS IsAvailable
                FROM Products p
                LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
                WHERE p.CategoryId = @CategoryId AND p.Id != @Id AND p.IsActive = 1
                GROUP BY p.Id, p.Name, p.SKU, p.ImageUrl
                ORDER BY SUM(ps.Stock) DESC";

                product.RelatedProducts = (await _connection.QueryAsync<RelatedProductDto>(relatedSql,
                    new { CategoryId = product.CategoryId, Id = id })).ToList();

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle del producto Id: {Id}", id);
                throw;
            }
        }

        private string DetermineSpecificationType(object? value)
        {
            if (value == null) return "text";

            return value switch
            {
                bool => "boolean",
                int or long or float or double or decimal => "number",
                _ => "text"
            };
        }

        // ===============================================
        // BÚSQUEDA AVANZADA CON FILTROS
        // ===============================================

        public async Task<PagedResponse<ProductDto>> GetProductsSearchAsync(ProductSearchRequest request)
        {
            try
            {
                var whereConditions = new List<string> { "p.IsActive = 1" };
                var parameters = new DynamicParameters();

                // Filtros de búsqueda
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    whereConditions.Add("(p.Name LIKE @SearchTerm OR p.Description LIKE @SearchTerm OR p.SKU LIKE @SearchTerm OR p.Brand LIKE @SearchTerm)");
                    parameters.Add("SearchTerm", $"%{request.SearchTerm}%");
                }

                if (request.CategoryId.HasValue)
                {
                    whereConditions.Add("p.CategoryId = @CategoryId");
                    parameters.Add("CategoryId", request.CategoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.Brand))
                {
                    whereConditions.Add("p.Brand = @Brand");
                    parameters.Add("Brand", request.Brand);
                }

                if (request.MinPrice.HasValue)
                {
                    whereConditions.Add("pricing.MinPrice >= @MinPrice");
                    parameters.Add("MinPrice", request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    whereConditions.Add("pricing.MaxPrice <= @MaxPrice");
                    parameters.Add("MaxPrice", request.MaxPrice.Value);
                }

                if (request.InStock == true)
                {
                    whereConditions.Add("ISNULL(pricing.TotalStock, 0) > 0");
                }

                var whereClause = string.Join(" AND ", whereConditions);
                var offset = (request.PageNumber - 1) * request.PageSize;

                // Query principal con paginación
                var dataSql = $@"
                SELECT 
                    p.Id, p.Name, p.Description, p.SKU, p.CategoryId, p.ImageUrl, p.Brand,
                    c.Name AS CategoryName,
                    ISNULL(pricing.MinPrice, 0) AS MinPrice,
                    ISNULL(pricing.MaxPrice, 0) AS MaxPrice,
                    ISNULL(pricing.TotalStock, 0) AS TotalStock,
                    ISNULL(pricing.SupplierCount, 0) AS SupplierCount,
                    CASE 
                        WHEN ISNULL(pricing.TotalStock, 0) = 0 THEN 'Sin Stock'
                        WHEN ISNULL(pricing.TotalStock, 0) <= 5 THEN 'Stock Bajo'
                        ELSE 'Disponible'
                    END AS StockStatus,
                    CASE WHEN ISNULL(pricing.TotalStock, 0) > 0 THEN 1 ELSE 0 END AS IsAvailable
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN (
                    SELECT 
                        ProductId,
                        MIN(Price) AS MinPrice,
                        MAX(Price) AS MaxPrice,
                        SUM(Stock) AS TotalStock,
                        COUNT(*) AS SupplierCount
                    FROM ProductSuppliers 
                    WHERE IsActive = 1
                    GROUP BY ProductId
                ) pricing ON p.Id = pricing.ProductId
                WHERE {whereClause}
                ORDER BY {GetOrderByClause(request.SortBy, request.SortDescending)}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                // Query de conteo
                var countSql = $@"
                SELECT COUNT(*)
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN (
                    SELECT 
                        ProductId,
                        MIN(Price) AS MinPrice,
                        MAX(Price) AS MaxPrice,
                        SUM(Stock) AS TotalStock,
                        COUNT(*) AS SupplierCount
                    FROM ProductSuppliers 
                    WHERE IsActive = 1
                    GROUP BY ProductId
                ) pricing ON p.Id = pricing.ProductId
                WHERE {whereClause}";

                parameters.Add("Offset", offset);
                parameters.Add("PageSize", request.PageSize);

                var products = await _connection.QueryAsync<ProductDto>(dataSql, parameters);
                var totalCount = await _connection.QuerySingleAsync<int>(countSql, parameters);

                _logger.LogInformation("Búsqueda de productos: {Count} resultados, página {Page}", totalCount, request.PageNumber);

                return PagedResponse<ProductDto>.Create(products, request.PageNumber, request.PageSize, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda avanzada de productos");
                throw;
            }
        }

        // ===============================================
        // VALIDACIONES Y UTILIDADES
        // ===============================================

        public async Task<bool> ExistsBySKUAsync(string sku, int? excludeId = null)
        {
            try
            {
                var sql = @"
                SELECT COUNT(1) FROM Products 
                WHERE LOWER(SKU) = LOWER(@SKU) 
                  AND IsActive = 1
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

                var count = await _connection.QuerySingleAsync<int>(sql, new { SKU = sku, ExcludeId = excludeId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto por SKU: {SKU}", sku);
                throw;
            }
        }

        public async Task<bool> HasSuppliersAsync(int productId)
        {
            try
            {
                var sql = "SELECT COUNT(1) FROM ProductSuppliers WHERE ProductId = @ProductId AND IsActive = 1";

                var count = await _connection.QuerySingleAsync<int>(sql, new { ProductId = productId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar proveedores para producto Id: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> HasWishlistEntriesAsync(int productId)
        {
            try
            {
                var sql = "SELECT COUNT(1) FROM Wishlists WHERE ProductId = @ProductId";

                var count = await _connection.QuerySingleAsync<int>(sql, new { ProductId = productId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar entradas de wishlist para producto Id: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> HasOrdersAsync(int productId)
        {
            try
            {
                var sql = @"
                SELECT COUNT(1) 
                FROM OrderDetails od
                INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
                WHERE ps.ProductId = @ProductId";

                var count = await _connection.QuerySingleAsync<int>(sql, new { ProductId = productId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar órdenes para producto Id: {ProductId}", productId);
                throw;
            }
        }

        // ===============================================
        // CONSULTAS ADICIONALES
        // ===============================================

        public async Task<IEnumerable<ProductCatalogDto>> GetProductsCatalogAsync(int? categoryId = null)
        {
            try
            {
                var whereClause = categoryId.HasValue ? "WHERE p.CategoryId = @CategoryId AND p.IsActive = 1" : "WHERE p.IsActive = 1";

                var sql = $@"
                SELECT 
                    p.Id, p.Name, p.Description, p.SKU, p.ImageUrl, p.Brand,
                    c.Name AS CategoryName,
                    MIN(ps.Price) AS MinPrice,
                    MAX(ps.Price) AS MaxPrice,
                    CASE WHEN SUM(ps.Stock) > 0 THEN 1 ELSE 0 END AS IsAvailable,
                    CASE 
                        WHEN SUM(ps.Stock) = 0 THEN 'Sin Stock'
                        WHEN SUM(ps.Stock) <= 5 THEN 'Stock Bajo'
                        ELSE 'Disponible'
                    END AS StockStatus,
                    COUNT(ps.Id) AS SupplierCount
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
                {whereClause}
                GROUP BY p.Id, p.Name, p.Description, p.SKU, p.ImageUrl, p.Brand, c.Name
                ORDER BY p.Name";

                var parameters = categoryId.HasValue ? new { CategoryId = categoryId.Value } : null;

                return await _connection.QueryAsync<ProductCatalogDto>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener catálogo de productos para categoría: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<RelatedProductDto>> GetRelatedProductsAsync(int productId, int limit = 5)
        {
            try
            {
                var sql = $@"
                SELECT TOP {limit}
                    p.Id, p.Name, p.SKU, p.ImageUrl,
                    MIN(ps.Price) AS MinPrice,
                    CASE WHEN SUM(ps.Stock) > 0 THEN 1 ELSE 0 END AS IsAvailable
                FROM Products p
                LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
                WHERE p.CategoryId = (SELECT CategoryId FROM Products WHERE Id = @ProductId) 
                  AND p.Id != @ProductId 
                  AND p.IsActive = 1
                GROUP BY p.Id, p.Name, p.SKU, p.ImageUrl
                ORDER BY SUM(ps.Stock) DESC, MIN(ps.Price) ASC";

                return await _connection.QueryAsync<RelatedProductDto>(sql, new { ProductId = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos relacionados para Id: {ProductId}", productId);
                throw;
            }
        }

        public async Task<ProductStatsDto> GetProductStatsAsync()
        {
            try
            {
                // Implementación básica de estadísticas
                var sql = @"
                SELECT 
                    COUNT(*) AS TotalProducts,
                    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveProducts
                FROM Products";

                var basicStats = await _connection.QueryFirstAsync<ProductStatsDto>(sql);

                return basicStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de productos");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            try
            {
                var sql = @"
                SELECT DISTINCT Brand 
                FROM Products 
                WHERE Brand IS NOT NULL AND Brand != '' AND IsActive = 1
                ORDER BY Brand";

                return await _connection.QueryAsync<string>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marcas de productos");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var sql = @"
                SELECT p.*, c.Name AS CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.CategoryId = @CategoryId AND p.IsActive = 1
                ORDER BY p.Name";

                return await _connection.QueryAsync<ProductDto>(sql, new { CategoryId = categoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByBrandAsync(string brand)
        {
            try
            {
                var sql = @"
                SELECT p.*, c.Name AS CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Brand = @Brand AND p.IsActive = 1
                ORDER BY p.Name";

                return await _connection.QueryAsync<ProductDto>(sql, new { Brand = brand });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por marca: {Brand}", brand);
                throw;
            }
        }

        // ===============================================
        // OVERRIDE SEARCH PARA PRODUCTOS
        // ===============================================

        protected override string GetWhereClause(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return "WHERE IsActive = 1";

            return @"WHERE IsActive = 1 
                 AND (Name LIKE @SearchTerm 
                      OR Description LIKE @SearchTerm
                      OR SKU LIKE @SearchTerm
                      OR Brand LIKE @SearchTerm)";
        }

        protected override string GetOrderByClause(string? sortBy, bool sortDescending)
        {
            var validSortColumns = new[] { "Name", "SKU", "Brand", "CreatedAt", "MinPrice", "TotalStock" };
            var sortColumn = validSortColumns.Contains(sortBy) ? sortBy : "Name";
            var sortDirection = sortDescending ? "DESC" : "ASC";
            return $"{sortColumn} {sortDirection}";
        }
    }
}
