using System.Text.Json;
using InvenBank.API.DTOs;

namespace InvenBank.API.Configuration
{
    /// <summary>
    /// Clase helper para validaciones y transformaciones específicas de productos
    /// Complementa los mapeos de AutoMapper con lógica de negocio específica
    /// </summary>
    public static class ProductMappingHelpers
    {
        // ===============================================
        // VALIDACIONES DE PRODUCTOS
        // ===============================================

        /// <summary>
        /// Valida si un SKU tiene el formato correcto
        /// </summary>
        /// <param name="sku">SKU a validar</param>
        /// <returns>True si el SKU es válido</returns>
        public static bool IsValidSKU(string? sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            // SKU debe tener entre 3 y 50 caracteres, solo letras, números y guiones
            if (sku.Length < 3 || sku.Length > 50)
                return false;

            // No puede empezar o terminar con guión
            if (sku.StartsWith('-') || sku.EndsWith('-'))
                return false;

            // Solo letras mayúsculas, números y guiones
            return sku.All(c => char.IsLetterOrDigit(c) || c == '-');
        }

        /// <summary>
        /// Normaliza un SKU al formato estándar
        /// </summary>
        /// <param name="sku">SKU a normalizar</param>
        /// <returns>SKU normalizado</returns>
        public static string NormalizeSKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU no puede estar vacío", nameof(sku));

            return sku.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Valida si las especificaciones JSON son válidas
        /// </summary>
        /// <param name="specifications">JSON de especificaciones</param>
        /// <returns>True si el JSON es válido</returns>
        public static bool IsValidSpecificationsJson(string? specifications)
        {
            if (string.IsNullOrWhiteSpace(specifications))
                return true; // Null/empty es válido

            try
            {
                JsonDocument.Parse(specifications);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        // ===============================================
        // TRANSFORMACIONES DE DATOS
        // ===============================================

        /// <summary>
        /// Convierte especificaciones JSON a lista de items estructurados
        /// </summary>
        /// <param name="specifications">JSON de especificaciones</param>
        /// <returns>Lista de especificaciones estructuradas</returns>
        public static List<ProductSpecificationItem> ParseSpecifications(string? specifications)
        {
            var items = new List<ProductSpecificationItem>();

            if (string.IsNullOrWhiteSpace(specifications))
                return items;

            try
            {
                var jsonDoc = JsonDocument.Parse(specifications);

                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    items.Add(new ProductSpecificationItem
                    {
                        Key = FormatSpecificationKey(property.Name),
                        Value = property.Value.ToString(),
                        Type = DetermineSpecificationType(property.Value)
                    });
                }
            }
            catch (JsonException)
            {
                // Si no se puede parsear, devolver lista vacía
                return items;
            }

            return items.OrderBy(x => x.Key).ToList();
        }

        /// <summary>
        /// Formatea una clave de especificación para mostrar
        /// </summary>
        /// <param name="key">Clave original</param>
        /// <returns>Clave formateada</returns>
        private static string FormatSpecificationKey(string key)
        {
            // Convertir camelCase o snake_case a formato legible
            var result = key;

            // Convertir camelCase a espacios
            result = System.Text.RegularExpressions.Regex.Replace(result, "([a-z])([A-Z])", "$1 $2");

            // Convertir snake_case a espacios
            result = result.Replace('_', ' ');

            // Capitalizar primera letra de cada palabra
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToLower());
        }

        /// <summary>
        /// Determina el tipo de una especificación basado en su valor JSON
        /// </summary>
        /// <param name="element">Elemento JSON</param>
        /// <returns>Tipo de especificación</returns>
        private static string DetermineSpecificationType(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Number => "number",
                JsonValueKind.String => "text",
                JsonValueKind.Array => "array",
                JsonValueKind.Object => "object",
                _ => "text"
            };
        }

        // ===============================================
        // CÁLCULOS DE ESTADO
        // ===============================================

        /// <summary>
        /// Determina el estado del stock basado en la cantidad total
        /// </summary>
        /// <param name="totalStock">Cantidad total en stock</param>
        /// <returns>Estado del stock</returns>
        public static string DetermineStockStatus(int totalStock)
        {
            return totalStock switch
            {
                0 => "Sin Stock",
                <= 5 => "Stock Bajo",
                <= 15 => "Stock Normal",
                <= 50 => "Stock Alto",
                _ => "Stock Abundante"
            };
        }

        /// <summary>
        /// Determina si un producto está disponible
        /// </summary>
        /// <param name="totalStock">Stock total</param>
        /// <param name="isActive">Si el producto está activo</param>
        /// <returns>True si está disponible</returns>
        public static bool IsProductAvailable(int totalStock, bool isActive)
        {
            return isActive && totalStock > 0;
        }

        /// <summary>
        /// Calcula el precio más competitivo (mejor precio) considerando disponibilidad
        /// </summary>
        /// <param name="prices">Lista de precios con stock</param>
        /// <returns>Mejor precio disponible o null si no hay stock</returns>
        public static decimal? CalculateBestPrice(IEnumerable<(decimal Price, int Stock)> prices)
        {
            var availablePrices = prices.Where(p => p.Stock > 0).Select(p => p.Price);
            return availablePrices.Any() ? availablePrices.Min() : null;
        }

        // ===============================================
        // GENERADORES DE DATOS
        // ===============================================

        /// <summary>
        /// Genera un SKU automático basado en nombre y categoría
        /// </summary>
        /// <param name="productName">Nombre del producto</param>
        /// <param name="categoryName">Nombre de la categoría</param>
        /// <returns>SKU generado</returns>
        public static string GenerateAutoSKU(string productName, string? categoryName = null)
        {
            var cleanName = CleanStringForSKU(productName);
            var cleanCategory = CleanStringForSKU(categoryName ?? "GEN");

            var namePrefix = cleanName.Length >= 3 ? cleanName[..3] : cleanName.PadRight(3, 'X');
            var categoryPrefix = cleanCategory.Length >= 2 ? cleanCategory[..2] : cleanCategory.PadRight(2, 'X');
            var timestamp = DateTime.Now.ToString("MMddHHmm");

            return $"{categoryPrefix}-{namePrefix}-{timestamp}";
        }

        /// <summary>
        /// Limpia una cadena para uso en SKU
        /// </summary>
        /// <param name="input">Cadena a limpiar</param>
        /// <returns>Cadena limpia para SKU</returns>
        private static string CleanStringForSKU(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "XXX";

            // Remover acentos y caracteres especiales, mantener solo alfanuméricos
            var result = input.ToUpperInvariant()
                .Replace(" ", "")
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace("Ñ", "N");

            return new string(result.Where(char.IsLetterOrDigit).ToArray());
        }

        // ===============================================
        // VALIDACIONES DE NEGOCIO
        // ===============================================

        /// <summary>
        /// Valida si un producto puede ser eliminado
        /// </summary>
        /// <param name="hasOrders">Si tiene órdenes asociadas</param>
        /// <param name="hasWishlistEntries">Si está en listas de deseos</param>
        /// <param name="hasSuppliers">Si tiene proveedores asociados</param>
        /// <returns>Resultado de validación</returns>
        public static (bool CanDelete, string Reason) CanDeleteProduct(
            bool hasOrders,
            bool hasWishlistEntries,
            bool hasSuppliers)
        {
            if (hasOrders)
                return (false, "No se puede eliminar un producto que tiene órdenes asociadas");

            if (hasSuppliers)
                return (false, "No se puede eliminar un producto que tiene proveedores asociados. Elimine primero las asociaciones.");

            if (hasWishlistEntries)
                return (false, "No se puede eliminar un producto que está en listas de deseos de usuarios");

            return (true, "El producto puede ser eliminado");
        }

        /// <summary>
        /// Valida los datos de un producto antes de crear/actualizar
        /// </summary>
        /// <param name="name">Nombre del producto</param>
        /// <param name="sku">SKU del producto</param>
        /// <param name="categoryId">ID de categoría</param>
        /// <param name="specifications">Especificaciones JSON</param>
        /// <returns>Lista de errores de validación</returns>
        public static List<string> ValidateProductData(
            string? name,
            string? sku,
            int categoryId,
            string? specifications)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
                errors.Add("El nombre del producto es requerido");
            else if (name.Length < 2 || name.Length > 200)
                errors.Add("El nombre debe tener entre 2 y 200 caracteres");

            if (string.IsNullOrWhiteSpace(sku))
                errors.Add("El SKU es requerido");
            else if (!IsValidSKU(sku))
                errors.Add("El SKU tiene un formato inválido");

            if (categoryId <= 0)
                errors.Add("Debe seleccionar una categoría válida");

            if (!IsValidSpecificationsJson(specifications))
                errors.Add("Las especificaciones deben estar en formato JSON válido");

            return errors;
        }
    }
}