using Dapper;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.DTOs.Responses;
using InvenBank.API.Repositories.Interfaces;
using System.Data;

namespace InvenBank.API.Repositories.Implementations
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly IDbConnection _connection;
        protected readonly ILogger<BaseRepository<T>> _logger;
        protected readonly string _tableName;
        protected readonly string _primaryKey;

        protected BaseRepository(IDbConnection connection, ILogger<BaseRepository<T>> logger, string tableName, string primaryKey = "Id")
        {
            _connection = connection;
            _logger = logger;
            _tableName = tableName;
            _primaryKey = primaryKey;
        }

        // ===============================================
        // MÉTODOS BÁSICOS CRUD
        // ===============================================

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                var sql = $"SELECT * FROM {_tableName} WHERE IsActive = 1 ORDER BY {_primaryKey}";

                _logger.LogInformation("Ejecutando consulta: {Sql}", sql);

                return await _connection.QueryAsync<T>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los registros de {TableName}", _tableName);
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                var sql = $"SELECT * FROM {_tableName} WHERE {_primaryKey} = @Id";

                _logger.LogInformation("Ejecutando consulta: {Sql} con Id: {Id}", sql, id);

                return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registro por Id {Id} de {TableName}", id, _tableName);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE {_primaryKey} = @Id";

                var count = await _connection.QuerySingleAsync<int>(sql, new { Id = id });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del registro Id {Id} en {TableName}", id, _tableName);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // Soft delete - marcar como inactivo
                var sql = $"UPDATE {_tableName} SET IsActive = 0, UpdatedAt = GETDATE() WHERE {_primaryKey} = @Id";

                _logger.LogInformation("Ejecutando eliminación lógica: {Sql} con Id: {Id}", sql, id);

                var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar registro Id {Id} de {TableName}", id, _tableName);
                throw;
            }
        }

        // ===============================================
        // MÉTODOS ABSTRACTOS PARA IMPLEMENTAR
        // ===============================================

        public abstract Task<int> CreateAsync(T entity);
        public abstract Task<bool> UpdateAsync(T entity);

        // ===============================================
        // PAGINACIÓN GENÉRICA
        // ===============================================

        public virtual async Task<PagedResponse<T>> GetPagedAsync(PaginationRequest request)
        {
            try
            {
                var offset = (request.PageNumber - 1) * request.PageSize;

                // Query base
                var baseQuery = GetBaseQuery();
                var whereClause = GetWhereClause(request.SearchTerm);
                var orderByClause = GetOrderByClause(request.SortBy, request.SortDescending);

                // Query para obtener datos paginados
                var dataQuery = $@"
                {baseQuery}
                {whereClause}
                {orderByClause}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                // Query para obtener total de registros
                var countQuery = $@"
                SELECT COUNT(*)
                FROM {_tableName}
                {whereClause.Replace("WHERE", "WHERE").Replace($"ORDER BY {orderByClause}", "")}";

                var parameters = new
                {
                    SearchTerm = $"%{request.SearchTerm}%",
                    Offset = offset,
                    PageSize = request.PageSize
                };

                // Ejecutar ambas queries
                var data = await _connection.QueryAsync<T>(dataQuery, parameters);
                var totalRecords = await _connection.QuerySingleAsync<int>(countQuery, parameters);

                _logger.LogInformation("Consulta paginada ejecutada. Página: {PageNumber}, Tamaño: {PageSize}, Total: {TotalRecords}",
                    request.PageNumber, request.PageSize, totalRecords);

                return PagedResponse<T>.Create(data, request.PageNumber, request.PageSize, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en consulta paginada de {TableName}", _tableName);
                throw;
            }
        }

        // ===============================================
        // MÉTODOS VIRTUALES PARA PERSONALIZACIÓN
        // ===============================================

        protected virtual string GetBaseQuery()
        {
            return $"SELECT * FROM {_tableName}";
        }

        protected virtual string GetWhereClause(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return "WHERE IsActive = 1";

            return "WHERE IsActive = 1"; // Override en clases derivadas para búsqueda específica
        }

        protected virtual string GetOrderByClause(string? sortBy, bool sortDescending)
        {
            var sortColumn = string.IsNullOrWhiteSpace(sortBy) ? _primaryKey : sortBy;
            var sortDirection = sortDescending ? "DESC" : "ASC";
            return $"ORDER BY {sortColumn} {sortDirection}";
        }

        // ===============================================
        // MÉTODOS DE UTILIDAD
        // ===============================================

        protected async Task<int> ExecuteScalarAsync(string sql, object? parameters = null)
        {
            try
            {
                _logger.LogInformation("Ejecutando ExecuteScalar: {Sql}", sql);
                return await _connection.QuerySingleAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ExecuteScalar: {Sql}", sql);
                throw;
            }
        }

        protected async Task<bool> ExecuteAsync(string sql, object? parameters = null)
        {
            try
            {
                _logger.LogInformation("Ejecutando Execute: {Sql}", sql);
                var affectedRows = await _connection.ExecuteAsync(sql, parameters);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Execute: {Sql}", sql);
                throw;
            }
        }

        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            try
            {
                _logger.LogInformation("Ejecutando QueryFirstOrDefault: {Sql}", sql);
                return await _connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en QueryFirstOrDefault: {Sql}", sql);
                throw;
            }
        }

        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            try
            {
                _logger.LogInformation("Ejecutando Query: {Sql}", sql);
                return await _connection.QueryAsync<T>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Query: {Sql}", sql);
                throw;
            }
        }
    }
}
