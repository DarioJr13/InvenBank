using Dapper;
using InvenBank.API.DTOs;
using InvenBank.API.Entities;
using InvenBank.API.Repositories.Interfaces;
using System.Data;

namespace InvenBank.API.Repositories.Implementations
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IDbConnection connection, ILogger<CategoryRepository> logger)
            : base(connection, logger, "Categories", "Id")
        {
        }

        // ===============================================
        // CRUD ESPECÍFICO PARA CATEGORÍAS
        // ===============================================

        public override async Task<int> CreateAsync(Category entity)
        {
            try
            {
                var sql = @"
                INSERT INTO Categories (Name, Description, ParentId, IsActive, CreatedAt, UpdatedAt)
                VALUES (@Name, @Description, @ParentId, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS int);";

                _logger.LogInformation("Creando nueva categoría: {Name}", entity.Name);

                var id = await _connection.QuerySingleAsync<int>(sql, entity);

                _logger.LogInformation("Categoría creada exitosamente con Id: {Id}", id);

                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría: {Name}", entity.Name);
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(Category entity)
        {
            try
            {
                var sql = @"
                UPDATE Categories 
                SET Name = @Name, 
                    Description = @Description, 
                    ParentId = @ParentId, 
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id";

                _logger.LogInformation("Actualizando categoría Id: {Id}", entity.Id);

                var affectedRows = await _connection.ExecuteAsync(sql, entity);

                var success = affectedRows > 0;
                _logger.LogInformation("Categoría actualizada: {Success}", success);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría Id: {Id}", entity.Id);
                throw;
            }
        }

        // ===============================================
        // CONSULTAS CON ESTADÍSTICAS (USANDO VIEWS)
        // ===============================================

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithStatsAsync()
        {
            try
            {
                var sql = @"
                SELECT 
                    c.Id,
                    c.Name,
                    c.Description,
                    c.ParentId,
                    p.Name AS ParentName,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt,
                    ISNULL(stats.ProductCount, 0) AS ProductCount,
                    ISNULL(stats.TotalStock, 0) AS TotalStock,
                    ISNULL(stats.TotalSold, 0) AS TotalSold
                FROM Categories c
                LEFT JOIN Categories p ON c.ParentId = p.Id
                LEFT JOIN vw_CategoryStats stats ON c.Id = stats.CategoryId
                WHERE c.IsActive = 1
                ORDER BY c.Name";

                _logger.LogInformation("Obteniendo categorías con estadísticas");

                var categories = await _connection.QueryAsync<CategoryDto>(sql);

                _logger.LogInformation("Obtenidas {Count} categorías con estadísticas", categories.Count());

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías con estadísticas");
                throw;
            }
        }

        public async Task<CategoryDto?> GetCategoryWithStatsAsync(int id)
        {
            try
            {
                var sql = @"
                SELECT 
                    c.Id,
                    c.Name,
                    c.Description,
                    c.ParentId,
                    p.Name AS ParentName,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt,
                    ISNULL(stats.ProductCount, 0) AS ProductCount,
                    ISNULL(stats.TotalStock, 0) AS TotalStock,
                    ISNULL(stats.TotalSold, 0) AS TotalSold
                FROM Categories c
                LEFT JOIN Categories p ON c.ParentId = p.Id
                LEFT JOIN vw_CategoryStats stats ON c.Id = stats.CategoryId
                WHERE c.Id = @Id";

                _logger.LogInformation("Obteniendo categoría con estadísticas Id: {Id}", id);

                var category = await _connection.QueryFirstOrDefaultAsync<CategoryDto>(sql, new { Id = id });

                if (category != null)
                {
                    _logger.LogInformation("Categoría encontrada: {Name}", category.Name);
                }
                else
                {
                    _logger.LogWarning("Categoría no encontrada Id: {Id}", id);
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría con estadísticas Id: {Id}", id);
                throw;
            }
        }

        // ===============================================
        // JERARQUÍA DE CATEGORÍAS
        // ===============================================

        public async Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync()
        {
            try
            {
                var sql = @"
                WITH CategoryHierarchy AS (
                    -- Categorías principales (padre)
                    SELECT 
                        c.Id,
                        c.Name,
                        c.Description,
                        c.ParentId,
                        CAST(NULL AS VARCHAR(100)) AS ParentName,
                        c.IsActive,
                        c.CreatedAt,
                        c.UpdatedAt,
                        0 AS Level,
                        CAST(c.Name AS VARCHAR(500)) AS FullPath,
                        ISNULL(stats.ProductCount, 0) AS ProductCount
                    FROM Categories c
                    LEFT JOIN vw_CategoryStats stats ON c.Id = stats.CategoryId
                    WHERE c.ParentId IS NULL AND c.IsActive = 1
                    
                    UNION ALL
                    
                    -- Subcategorías (hijos)
                    SELECT 
                        c.Id,
                        c.Name,
                        c.Description,
                        c.ParentId,
                        ch.Name AS ParentName,
                        c.IsActive,
                        c.CreatedAt,
                        c.UpdatedAt,
                        ch.Level + 1,
                        CAST(ch.FullPath + ' > ' + c.Name AS VARCHAR(500)),
                        ISNULL(stats.ProductCount, 0) AS ProductCount
                    FROM Categories c
                    INNER JOIN CategoryHierarchy ch ON c.ParentId = ch.Id
                    LEFT JOIN vw_CategoryStats stats ON c.Id = stats.CategoryId
                    WHERE c.IsActive = 1
                )
                SELECT * FROM CategoryHierarchy
                ORDER BY FullPath";

                _logger.LogInformation("Obteniendo jerarquía completa de categorías");

                var hierarchy = await _connection.QueryAsync<CategoryDto>(sql);

                _logger.LogInformation("Obtenida jerarquía con {Count} categorías", hierarchy.Count());

                return hierarchy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener jerarquía de categorías");
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesByParentAsync(int? parentId)
        {
            try
            {
                var sql = @"
                SELECT * FROM Categories 
                WHERE (@ParentId IS NULL AND ParentId IS NULL) 
                   OR (@ParentId IS NOT NULL AND ParentId = @ParentId)
                   AND IsActive = 1
                ORDER BY Name";

                _logger.LogInformation("Obteniendo categorías por padre: {ParentId}", parentId?.ToString() ?? "NULL");

                var categories = await _connection.QueryAsync<Category>(sql, new { ParentId = parentId });

                _logger.LogInformation("Obtenidas {Count} categorías hijas", categories.Count());

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías por padre: {ParentId}", parentId);
                throw;
            }
        }

        // ===============================================
        // VALIDACIONES DE NEGOCIO
        // ===============================================

        public async Task<bool> HasChildCategoriesAsync(int categoryId)
        {
            try
            {
                var sql = "SELECT COUNT(1) FROM Categories WHERE ParentId = @CategoryId AND IsActive = 1";

                var count = await _connection.QuerySingleAsync<int>(sql, new { CategoryId = categoryId });

                _logger.LogInformation("Categoría {CategoryId} tiene {Count} hijos", categoryId, count);

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar categorías hijas para Id: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            try
            {
                var sql = "SELECT COUNT(1) FROM Products WHERE CategoryId = @CategoryId AND IsActive = 1";

                var count = await _connection.QuerySingleAsync<int>(sql, new { CategoryId = categoryId });

                _logger.LogInformation("Categoría {CategoryId} tiene {Count} productos", categoryId, count);

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar productos para categoría Id: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            try
            {
                var sql = @"
                SELECT COUNT(1) FROM Categories 
                WHERE LOWER(Name) = LOWER(@Name) 
                  AND IsActive = 1
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

                var count = await _connection.QuerySingleAsync<int>(sql, new { Name = name, ExcludeId = excludeId });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría por nombre: {Name}", name);
                throw;
            }
        }

        // ===============================================
        // OVERRIDE SEARCH PARA CATEGORÍAS
        // ===============================================

        protected override string GetWhereClause(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return "WHERE IsActive = 1";

            return @"WHERE IsActive = 1 
                 AND (Name LIKE @SearchTerm 
                      OR Description LIKE @SearchTerm)";
        }
    }
}
