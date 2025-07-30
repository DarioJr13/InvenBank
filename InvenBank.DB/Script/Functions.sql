-- ===============================================
-- FUNCTIONS - SISTEMA INVENBANK
-- Propósito: Cálculos de negocio y utilidades
-- ===============================================

USE InvenBank_db;
GO

-- ===============================================
-- 1. FUNCIONES DE PRECIOS Y COSTOS
-- ===============================================

-- Función para obtener el precio mínimo de un producto
CREATE OR ALTER FUNCTION fn_GetMinPrice(@ProductId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @MinPrice DECIMAL(18,2);
    
    SELECT @MinPrice = MIN(Price)
    FROM ProductSuppliers 
    WHERE ProductId = @ProductId 
      AND IsActive = 1 
      AND Stock > 0;
    
    RETURN ISNULL(@MinPrice, 0);
END
GO

-- Función para obtener el precio máximo de un producto
CREATE OR ALTER FUNCTION fn_GetMaxPrice(@ProductId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @MaxPrice DECIMAL(18,2);
    
    SELECT @MaxPrice = MAX(Price)
    FROM ProductSuppliers 
    WHERE ProductId = @ProductId 
      AND IsActive = 1 
      AND Stock > 0;
    
    RETURN ISNULL(@MaxPrice, 0);
END
GO

-- Función para calcular precio promedio ponderado por stock
CREATE OR ALTER FUNCTION fn_GetWeightedAvgPrice(@ProductId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @WeightedAvg DECIMAL(18,2);
    
    SELECT @WeightedAvg = 
        SUM(Price * Stock) / NULLIF(SUM(Stock), 0)
    FROM ProductSuppliers 
    WHERE ProductId = @ProductId 
      AND IsActive = 1 
      AND Stock > 0;
    
    RETURN ISNULL(@WeightedAvg, 0);
END
GO

-- Función para calcular el mejor precio (más barato con stock)
CREATE OR ALTER FUNCTION fn_GetBestPrice(@ProductId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @BestPrice DECIMAL(18,2);
    
    SELECT TOP 1 @BestPrice = Price
    FROM ProductSuppliers 
    WHERE ProductId = @ProductId 
      AND IsActive = 1 
      AND Stock > 0
    ORDER BY Price ASC, Stock DESC; -- Precio más bajo, mayor stock como desempate
    
    RETURN ISNULL(@BestPrice, 0);
END
GO

-- Función para obtener información del proveedor con mejor precio
CREATE OR ALTER FUNCTION fn_GetBestPriceSupplier(@ProductId INT)
RETURNS TABLE
AS
RETURN (
    SELECT TOP 1 
        s.Id AS SupplierId,
        s.Name AS SupplierName,
        ps.Price,
        ps.Stock,
        ps.Id AS ProductSupplierId
    FROM ProductSuppliers ps
    INNER JOIN Suppliers s ON ps.SupplierId = s.Id
    WHERE ps.ProductId = @ProductId 
      AND ps.IsActive = 1 
      AND ps.Stock > 0
      AND s.IsActive = 1
    ORDER BY ps.Price ASC, ps.Stock DESC
);
GO

-- ===============================================
-- 2. FUNCIONES DE STOCK E INVENTARIO
-- ===============================================

-- Función para calcular stock total de un producto
CREATE OR ALTER FUNCTION fn_GetTotalStock(@ProductId INT)
RETURNS INT
AS
BEGIN
    DECLARE @TotalStock INT;
    
    SELECT @TotalStock = SUM(Stock)
    FROM ProductSuppliers 
    WHERE ProductId = @ProductId 
      AND IsActive = 1;
    
    RETURN ISNULL(@TotalStock, 0);
END
GO

-- Función para obtener el estado del stock
CREATE OR ALTER FUNCTION fn_GetStockStatus(@Stock INT)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @Status VARCHAR(20);
    
    SET @Status = CASE 
        WHEN @Stock = 0 THEN 'Agotado'
        WHEN @Stock <= 5 THEN 'Crítico'
        WHEN @Stock <= 15 THEN 'Bajo'
        WHEN @Stock <= 50 THEN 'Normal'
        ELSE 'Alto'
    END;
    
    RETURN @Status;
END
GO

-- Función para verificar disponibilidad de stock para una cantidad específica
CREATE OR ALTER FUNCTION fn_IsStockAvailable(@ProductId INT, @RequiredQuantity INT)
RETURNS BIT
AS
BEGIN
    DECLARE @AvailableStock INT;
    
    SELECT @AvailableStock = SUM(Stock)
    FROM ProductSuppliers 
    WHERE ProductId = @ProductId 
      AND IsActive = 1;
    
    RETURN CASE WHEN ISNULL(@AvailableStock, 0) >= @RequiredQuantity THEN 1 ELSE 0 END;
END
GO

-- Función para obtener proveedores con stock disponible para un producto
CREATE OR ALTER FUNCTION fn_GetSuppliersWithStock(@ProductId INT, @MinStock INT = 1)
RETURNS TABLE
AS
RETURN (
    SELECT 
        s.Id AS SupplierId,
        s.Name AS SupplierName,
        ps.Price,
        ps.Stock,
        ps.BatchNumber,
        ps.Id AS ProductSupplierId
    FROM ProductSuppliers ps
    INNER JOIN Suppliers s ON ps.SupplierId = s.Id
    WHERE ps.ProductId = @ProductId 
      AND ps.IsActive = 1 
      AND ps.Stock >= @MinStock
      AND s.IsActive = 1
);
GO

-- ===============================================
-- 3. FUNCIONES DE VALIDACIÓN
-- ===============================================

-- Función para validar formato de SKU
CREATE OR ALTER FUNCTION fn_IsValidSKU(@SKU VARCHAR(50))
RETURNS BIT
AS
BEGIN
    DECLARE @IsValid BIT = 0;
    
    -- SKU debe tener entre 3 y 50 caracteres, solo letras, números y guiones
    IF LEN(@SKU) BETWEEN 3 AND 50 
       AND @SKU NOT LIKE '%[^A-Za-z0-9-]%'
       AND @SKU NOT LIKE '-%'  -- No puede empezar con guión
       AND @SKU NOT LIKE '%-'  -- No puede terminar con guión
    BEGIN
        SET @IsValid = 1;
    END
    
    RETURN @IsValid;
END
GO

-- Función para validar email
CREATE OR ALTER FUNCTION fn_IsValidEmail(@Email VARCHAR(255))
RETURNS BIT
AS
BEGIN
    DECLARE @IsValid BIT = 0;
    
    -- Validación básica de email
    IF @Email LIKE '%_@_%.__%' 
       AND @Email NOT LIKE '%@%@%'  -- No doble @
       AND @Email NOT LIKE '%..'    -- No doble punto
       AND LEN(@Email) >= 5
       AND LEN(@Email) <= 255
    BEGIN
        SET @IsValid = 1;
    END
    
    RETURN @IsValid;
END
GO

-- Función para validar precio
CREATE OR ALTER FUNCTION fn_IsValidPrice(@Price DECIMAL(18,2))
RETURNS BIT
AS
BEGIN
    RETURN CASE 
        WHEN @Price > 0 AND @Price <= 999999.99 THEN 1 
        ELSE 0 
    END;
END
GO

-- ===============================================
-- 4. FUNCIONES DE ESTADÍSTICAS Y REPORTES
-- ===============================================

-- Función para calcular total de ventas de un producto
CREATE OR ALTER FUNCTION fn_GetProductTotalSales(@ProductId INT, @StartDate DATETIME2 = NULL, @EndDate DATETIME2 = NULL)
RETURNS INT
AS
BEGIN
    DECLARE @TotalSales INT;
    
    IF @StartDate IS NULL SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL SET @EndDate = GETDATE();
    
    SELECT @TotalSales = SUM(od.Quantity)
    FROM OrderDetails od
    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
    INNER JOIN Orders o ON od.OrderId = o.Id
    WHERE ps.ProductId = @ProductId
      AND o.OrderDate BETWEEN @StartDate AND @EndDate
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered');
    
    RETURN ISNULL(@TotalSales, 0);
END
GO

-- Función para calcular ingresos totales de un producto
CREATE OR ALTER FUNCTION fn_GetProductTotalRevenue(@ProductId INT, @StartDate DATETIME2 = NULL, @EndDate DATETIME2 = NULL)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @TotalRevenue DECIMAL(18,2);
    
    IF @StartDate IS NULL SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL SET @EndDate = GETDATE();
    
    SELECT @TotalRevenue = SUM(od.TotalPrice)
    FROM OrderDetails od
    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
    INNER JOIN Orders o ON od.OrderId = o.Id
    WHERE ps.ProductId = @ProductId
      AND o.OrderDate BETWEEN @StartDate AND @EndDate
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered');
    
    RETURN ISNULL(@TotalRevenue, 0);
END
GO

-- Función para obtener ranking de productos más vendidos
CREATE OR ALTER FUNCTION fn_GetTopSellingProducts(@TopCount INT = 10, @Days INT = 30)
RETURNS TABLE
AS
RETURN (
    SELECT TOP (@TopCount)
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.SKU,
        SUM(od.Quantity) AS TotalSold,
        SUM(od.TotalPrice) AS TotalRevenue,
        AVG(od.UnitPrice) AS AvgPrice
    FROM OrderDetails od
    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
    INNER JOIN Products p ON ps.ProductId = p.Id
    INNER JOIN Orders o ON od.OrderId = o.Id
    WHERE o.OrderDate >= DATEADD(DAY, -@Days, GETDATE())
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    GROUP BY p.Id, p.Name, p.SKU
    ORDER BY SUM(od.Quantity) DESC
);
GO

-- Función para calcular margen de ganancia estimado
CREATE OR ALTER FUNCTION fn_CalculateMargin(@SellingPrice DECIMAL(18,2), @MarginPercent DECIMAL(5,2) = 30.0)
RETURNS DECIMAL(18,2)
AS
BEGIN
    RETURN @SellingPrice * (@MarginPercent / 100.0);
END
GO

-- ===============================================
-- 5. FUNCIONES DE FORMATEO Y UTILIDADES
-- ===============================================

-- Función para formatear precios como moneda
CREATE OR ALTER FUNCTION fn_FormatPrice(@Price DECIMAL(18,2), @Currency VARCHAR(3) = 'USD')
RETURNS VARCHAR(50)
AS
BEGIN
    DECLARE @FormattedPrice VARCHAR(50);
    
    SET @FormattedPrice = @Currency + ' ' + FORMAT(@Price, 'N2');
    
    RETURN @FormattedPrice;
END
GO

-- Función para generar código de orden único
CREATE OR ALTER FUNCTION fn_GenerateOrderNumber(@Date DATETIME2 = NULL)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @OrderNumber VARCHAR(20);
    DECLARE @OrderDate DATE;
    DECLARE @Counter INT;
    
    IF @Date IS NULL SET @Date = GETDATE();
    SET @OrderDate = CAST(@Date AS DATE);
    
    -- Obtener el contador del día
    SELECT @Counter = COUNT(*) + 1
    FROM Orders 
    WHERE CAST(OrderDate AS DATE) = @OrderDate;
    
    -- Formato: ORD-YYYYMMDD-0001
    SET @OrderNumber = 'ORD-' + FORMAT(@OrderDate, 'yyyyMMdd') + '-' + FORMAT(@Counter, '0000');
    
    RETURN @OrderNumber;
END
GO

-- Función para generar SKU automático
CREATE OR ALTER FUNCTION fn_GenerateAutoSKU(@CategoryId INT, @ProductName VARCHAR(200))
RETURNS VARCHAR(50)
AS
BEGIN
    DECLARE @SKU VARCHAR(50);
    DECLARE @CategoryCode VARCHAR(10);
    DECLARE @ProductCode VARCHAR(20);
    DECLARE @Counter INT;
    
    -- Obtener código de categoría (primeras 3 letras)
    SELECT @CategoryCode = UPPER(LEFT(REPLACE(Name, ' ', ''), 3))
    FROM Categories 
    WHERE Id = @CategoryId;
    
    -- Obtener código de producto (primeras 5 letras del nombre)
    SET @ProductCode = UPPER(LEFT(REPLACE(@ProductName, ' ', ''), 5));
    
    -- Obtener contador
    SELECT @Counter = COUNT(*) + 1
    FROM Products 
    WHERE CategoryId = @CategoryId;
    
    -- Formato: CAT-PROD-001
    SET @SKU = @CategoryCode + '-' + @ProductCode + '-' + FORMAT(@Counter, '000');
    
    RETURN @SKU;
END
GO

-- ===============================================
-- 6. FUNCIONES DE ANÁLISIS TEMPORAL
-- ===============================================

-- Función para obtener ventas por periodo
CREATE OR ALTER FUNCTION fn_GetSalesByPeriod(@StartDate DATETIME2, @EndDate DATETIME2)
RETURNS TABLE
AS
RETURN (
    SELECT 
        CAST(o.OrderDate AS DATE) AS SaleDate,
        COUNT(DISTINCT o.Id) AS OrderCount,
        SUM(o.TotalAmount) AS TotalRevenue,
        AVG(o.TotalAmount) AS AvgOrderValue,
        COUNT(DISTINCT o.UserId) AS UniqueCustomers
    FROM Orders o
    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    GROUP BY CAST(o.OrderDate AS DATE)
);
GO

-- Función para comparar ventas periodo vs periodo anterior
CREATE OR ALTER FUNCTION fn_ComparePeriodSales(@CurrentStart DATETIME2, @CurrentEnd DATETIME2, @PreviousStart DATETIME2, @PreviousEnd DATETIME2)
RETURNS TABLE
AS
RETURN (
    SELECT 
        'Current' AS Period,
        COUNT(DISTINCT o.Id) AS OrderCount,
        SUM(o.TotalAmount) AS TotalRevenue,
        AVG(o.TotalAmount) AS AvgOrderValue
    FROM Orders o
    WHERE o.OrderDate BETWEEN @CurrentStart AND @CurrentEnd
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    
    UNION ALL
    
    SELECT 
        'Previous' AS Period,
        COUNT(DISTINCT o.Id) AS OrderCount,
        SUM(o.TotalAmount) AS TotalRevenue,
        AVG(o.TotalAmount) AS AvgOrderValue
    FROM Orders o
    WHERE o.OrderDate BETWEEN @PreviousStart AND @PreviousEnd
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered')
);
GO

-- ===============================================
-- 7. FUNCIÓN DE RECOMENDACIONES
-- ===============================================

-- Función para obtener productos relacionados/recomendados
CREATE OR ALTER FUNCTION fn_GetRelatedProducts(@ProductId INT, @TopCount INT = 5)
RETURNS TABLE
AS
RETURN (
    -- Productos de la misma categoría, ordenados por popularidad
    SELECT TOP (@TopCount)
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.SKU,
        dbo.fn_GetBestPrice(p.Id) AS BestPrice,
        dbo.fn_GetTotalStock(p.Id) AS TotalStock,
        ISNULL(sales.TotalSold, 0) AS TotalSold
    FROM Products p
    LEFT JOIN (
        SELECT 
            ps.ProductId,
            SUM(od.Quantity) AS TotalSold
        FROM OrderDetails od
        INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
        INNER JOIN Orders o ON od.OrderId = o.Id
        WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
        GROUP BY ps.ProductId
    ) sales ON p.Id = sales.ProductId
    WHERE p.CategoryId = (SELECT CategoryId FROM Products WHERE Id = @ProductId)
      AND p.Id != @ProductId
      AND p.IsActive = 1
      AND dbo.fn_GetTotalStock(p.Id) > 0
    ORDER BY ISNULL(sales.TotalSold, 0) DESC, dbo.fn_GetBestPrice(p.Id) ASC
);
GO

PRINT '';
PRINT '===============================================';
PRINT 'FUNCTIONS CREADAS EXITOSAMENTE';
PRINT '===============================================';
PRINT 'Módulos de funciones implementados:';
PRINT '- Precios y Costos (5 functions)';
PRINT '- Stock e Inventario (4 functions)';
PRINT '- Validación (3 functions)';
PRINT '- Estadísticas y Reportes (4 functions)';
PRINT '- Formateo y Utilidades (3 functions)';
PRINT '- Análisis Temporal (2 functions)';
PRINT '- Recomendaciones (1 function)';
PRINT '';
PRINT 'Total: 22 Functions especializadas';
PRINT '';
PRINT '🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉';
PRINT '         BASE DE DATOS COMPLETADA           ';
PRINT '🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉';
PRINT '';
PRINT '✅ RESUMEN FINAL DE IMPLEMENTACIÓN:';
PRINT '';
PRINT '📋 DDL Scripts: 11 tablas + índices + datos iniciales';
PRINT '⚙️  Stored Procedures: 13 procedimientos CRUD + negocio';
PRINT '👁️  Views: 9 vistas especializadas para consultas';
PRINT '🔧 Triggers: 12 triggers de auditoría y validación';
PRINT '🧮 Functions: 22 funciones de cálculo y utilidades';
PRINT '';
PRINT '📊 CARACTERÍSTICAS IMPLEMENTADAS:';
PRINT '✅ Sistema multi-proveedor con precios diferenciados';
PRINT '✅ Gestión completa de inventario por lotes';
PRINT '✅ Sistema de órdenes con validación de stock';
PRINT '✅ Lista de deseos (wishlist) para usuarios';
PRINT '✅ Auditoría completa de operaciones críticas';
PRINT '✅ Alertas automáticas de stock bajo';
PRINT '✅ Reportes y estadísticas de ventas';
PRINT '✅ Validaciones de negocio robustas';
PRINT '✅ Funciones de recomendación de productos';
PRINT '';
PRINT '===============================================';
GO