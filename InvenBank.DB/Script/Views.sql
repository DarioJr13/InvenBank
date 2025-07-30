-- ===============================================
-- VIEWS - SISTEMA INVENBANK
-- Propósito: Consultas frecuentes y reportes
-- ===============================================

USE InvenBank_db;
GO

-- ===============================================
-- 1. VIEW: PRODUCTOS CON INFORMACIÓN AGREGADA
-- ===============================================

-- Vista principal de productos con precios y stock
CREATE OR ALTER VIEW vw_ProductsCatalog
AS
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    CAST(p.Description AS VARCHAR(MAX)) AS Description,
    p.SKU,
    p.Brand,
    p.ImageUrl,
    CAST(p.Specifications AS VARCHAR(MAX)) AS Specifications,
    c.Id AS CategoryId,
    c.Name AS CategoryName,
    c.ParentId AS ParentCategoryId,
    
    -- Información de precios
    MIN(ps.Price) AS MinPrice,
    MAX(ps.Price) AS MaxPrice,
    AVG(ps.Price) AS AvgPrice,
    
    -- Información de stock
    SUM(ps.Stock) AS TotalStock,
    COUNT(ps.Id) AS SupplierCount,
    
    -- Estado del producto
    CASE 
        WHEN SUM(ps.Stock) = 0 THEN 'Sin Stock'
        WHEN SUM(ps.Stock) <= 10 THEN 'Stock Bajo'
        ELSE 'Disponible'
    END AS StockStatus,
    
    -- Información de disponibilidad
    CASE WHEN SUM(ps.Stock) > 0 THEN 1 ELSE 0 END AS IsAvailable,
    
    -- Metadatos
    p.IsActive,
    p.CreatedAt,
    p.UpdatedAt,
    
    -- Proveedor con mejor precio
    (SELECT TOP 1 s.Name 
     FROM ProductSuppliers ps2 
     INNER JOIN Suppliers s ON ps2.SupplierId = s.Id
     WHERE ps2.ProductId = p.Id AND ps2.IsActive = 1
     ORDER BY ps2.Price ASC) AS BestPriceSupplier

FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
WHERE p.IsActive = 1
GROUP BY 
    p.Id, p.Name, CAST(p.Description AS VARCHAR(MAX)), p.SKU, p.Brand, p.ImageUrl, CAST(p.Specifications AS VARCHAR(MAX)),
    c.Id, c.Name, c.ParentId, p.IsActive, p.CreatedAt, p.UpdatedAt;
GO

-- ===============================================
-- 2. VIEW: INVENTARIO DETALLADO
-- ===============================================

CREATE OR ALTER VIEW vw_DetailedInventory
AS
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU,
    p.Brand,
    c.Name AS CategoryName,
    
    s.Id AS SupplierId,
    s.Name AS SupplierName,
    s.ContactPerson AS SupplierContact,
    
    ps.Id AS ProductSupplierId,
    ps.Price,
    ps.Stock,
    ps.BatchNumber,
    ps.SupplierSKU,
    ps.LastRestockDate,
    
    -- Clasificación de stock
    CASE 
        WHEN ps.Stock = 0 THEN 'Agotado'
        WHEN ps.Stock <= 5 THEN 'Crítico'
        WHEN ps.Stock <= 15 THEN 'Bajo'
        ELSE 'Normal'
    END AS StockLevel,
    
    -- Valor del inventario
    (ps.Stock * ps.Price) AS InventoryValue,
    
    -- Días desde último restock
    DATEDIFF(DAY, ps.LastRestockDate, GETDATE()) AS DaysSinceRestock,
    
    ps.IsActive AS IsActive,
    ps.CreatedAt,
    ps.UpdatedAt

FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
INNER JOIN ProductSuppliers ps ON p.Id = ps.ProductId
INNER JOIN Suppliers s ON ps.SupplierId = s.Id
WHERE p.IsActive = 1 AND ps.IsActive = 1 AND s.IsActive = 1;
GO

-- ===============================================
-- 3. VIEW: ÓRDENES CON DETALLES COMPLETOS
-- ===============================================

CREATE OR ALTER VIEW vw_OrdersComplete
AS
SELECT 
    o.Id AS OrderId,
    o.OrderNumber,
    o.OrderDate,
    o.Status AS OrderStatus,
    o.TotalAmount,
    o.DeliveryDate,
    
    -- Información del cliente
    u.Id AS CustomerId,
    u.FirstName + ' ' + u.LastName AS CustomerName,
    u.Email AS CustomerEmail,
    
    -- Detalles del pedido
    od.Id AS OrderDetailId,
    od.Quantity,
    od.UnitPrice,
    od.TotalPrice AS LineTotal,
    
    -- Información del producto
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU,
    p.Brand,
    
    -- Información del proveedor
    s.Id AS SupplierId,
    s.Name AS SupplierName,
    ps.BatchNumber,
    
    -- Metadatos
    o.ShippingAddress,
    o.Notes AS OrderNotes,
    o.CreatedAt AS OrderCreatedAt

FROM Orders o
INNER JOIN Users u ON o.UserId = u.Id
INNER JOIN OrderDetails od ON o.Id = od.OrderId
INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
INNER JOIN Products p ON ps.ProductId = p.Id
INNER JOIN Suppliers s ON ps.SupplierId = s.Id;
GO

-- ===============================================
-- 4. VIEW: ESTADÍSTICAS DE VENTAS
-- ===============================================

CREATE OR ALTER VIEW vw_SalesStats
AS
SELECT 
    -- Agrupación temporal
    YEAR(o.OrderDate) AS Year,
    MONTH(o.OrderDate) AS Month,
    DATENAME(MONTH, o.OrderDate) AS MonthName,
    
    -- Información del producto
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU,
    c.Name AS CategoryName,
    
    -- Información del proveedor
    s.Id AS SupplierId,
    s.Name AS SupplierName,
    
    -- Métricas de ventas
    COUNT(DISTINCT o.Id) AS OrderCount,
    SUM(od.Quantity) AS TotalQuantitySold,
    SUM(od.TotalPrice) AS TotalRevenue,
    AVG(od.UnitPrice) AS AvgSellingPrice,
    COUNT(DISTINCT o.UserId) AS UniqueCustomers,
    
    -- Métricas de rentabilidad (asumiendo margen)
    SUM(od.TotalPrice) * 0.3 AS EstimatedProfit -- 30% margen estimado

FROM Orders o
INNER JOIN OrderDetails od ON o.Id = od.OrderId
INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
INNER JOIN Products p ON ps.ProductId = p.Id
INNER JOIN Categories c ON p.CategoryId = c.Id
INNER JOIN Suppliers s ON ps.SupplierId = s.Id
WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
GROUP BY 
    YEAR(o.OrderDate), MONTH(o.OrderDate), DATENAME(MONTH, o.OrderDate),
    p.Id, p.Name, p.SKU, c.Name, s.Id, s.Name;
GO

-- ===============================================
-- 5. VIEW: PRODUCTOS MÁS DESEADOS (WISHLIST)
-- ===============================================

CREATE OR ALTER VIEW vw_MostWishedProducts
AS
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU,
    p.Brand,
    p.ImageUrl,
    c.Name AS CategoryName,
    
    -- Estadísticas de wishlist
    COUNT(w.Id) AS WishlistCount,
    COUNT(DISTINCT w.UserId) AS UniqueWishers,
    
    -- Información de precios actuales
    MIN(ps.Price) AS MinPrice,
    MAX(ps.Price) AS MaxPrice,
    SUM(ps.Stock) AS TotalStock,
    
    -- Fecha del primer y último agregado a wishlist
    MIN(w.AddedDate) AS FirstWishDate,
    MAX(w.AddedDate) AS LastWishDate,
    
    -- Disponibilidad
    CASE WHEN SUM(ps.Stock) > 0 THEN 1 ELSE 0 END AS IsAvailable,
    
    -- Ratio de conversión (wishlist a compra) - aproximado
    CAST(
        (SELECT COUNT(DISTINCT od.OrderId) 
         FROM OrderDetails od 
         INNER JOIN ProductSuppliers ps2 ON od.ProductSupplierId = ps2.Id 
         WHERE ps2.ProductId = p.Id) AS FLOAT
    ) / COUNT(w.Id) AS ConversionRate

FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
INNER JOIN Wishlists w ON p.Id = w.ProductId
LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
WHERE p.IsActive = 1
GROUP BY p.Id, p.Name, p.SKU, p.Brand, p.ImageUrl, c.Name
HAVING COUNT(w.Id) > 0;
GO

-- ===============================================
-- 6. VIEW: ESTADÍSTICAS DE PROVEEDORES
-- ===============================================

CREATE OR ALTER VIEW vw_SupplierStats
AS
SELECT 
    s.Id AS SupplierId,
    s.Name AS SupplierName,
    s.ContactPerson,
    s.Email,
    s.Phone,
    
    -- Estadísticas de productos
    COUNT(DISTINCT ps.ProductId) AS ProductCount,
    COUNT(ps.Id) AS TotalAssociations,
    
    -- Estadísticas de precios
    MIN(ps.Price) AS MinPrice,
    MAX(ps.Price) AS MaxPrice,
    AVG(ps.Price) AS AvgPrice,
    
    -- Estadísticas de stock
    SUM(ps.Stock) AS TotalStock,
    SUM(ps.Stock * ps.Price) AS TotalInventoryValue,
    
    -- Estadísticas de ventas
    ISNULL(sales.TotalSales, 0) AS TotalSales,
    ISNULL(sales.TotalRevenue, 0) AS TotalRevenue,
    ISNULL(sales.OrderCount, 0) AS OrderCount,
    
    -- Última actividad
    MAX(ps.UpdatedAt) AS LastUpdate,
    MAX(ps.LastRestockDate) AS LastRestock,
    
    -- Estado
    s.IsActive,
    s.CreatedAt

FROM Suppliers s
LEFT JOIN ProductSuppliers ps ON s.Id = ps.SupplierId AND ps.IsActive = 1
LEFT JOIN (
    SELECT 
        ps.SupplierId,
        SUM(od.Quantity) AS TotalSales,
        SUM(od.TotalPrice) AS TotalRevenue,
        COUNT(DISTINCT od.OrderId) AS OrderCount
    FROM OrderDetails od
    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
    INNER JOIN Orders o ON od.OrderId = o.Id
    WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    GROUP BY ps.SupplierId
) sales ON s.Id = sales.SupplierId
WHERE s.IsActive = 1
GROUP BY 
    s.Id, s.Name, s.ContactPerson, s.Email, s.Phone, s.IsActive, s.CreatedAt,
    sales.TotalSales, sales.TotalRevenue, sales.OrderCount;
GO

-- ===============================================
-- 7. VIEW: CATEGORÍAS CON ESTADÍSTICAS
-- ===============================================

CREATE OR ALTER VIEW vw_CategoryStats
AS
WITH CategoryHierarchy AS (
    -- Categorías principales
    SELECT 
        Id, Name, ParentId, 0 AS Level,
        CAST(Name AS VARCHAR(500)) AS FullPath
    FROM Categories 
    WHERE ParentId IS NULL
    
    UNION ALL
    
    -- Subcategorías
    SELECT 
        c.Id, c.Name, c.ParentId, ch.Level + 1,
        CAST(ch.FullPath + ' > ' + c.Name AS VARCHAR(500))
    FROM Categories c
    INNER JOIN CategoryHierarchy ch ON c.ParentId = ch.Id
)
SELECT 
    c.Id AS CategoryId,
    c.Name AS CategoryName,
    c.ParentId,
    ch.Level,
    ch.FullPath,
    
    -- Estadísticas de productos
    COUNT(p.Id) AS ProductCount,
    COUNT(CASE WHEN p.IsActive = 1 THEN 1 END) AS ActiveProductCount,
    
    -- Estadísticas de stock
    SUM(CASE WHEN ps.Stock IS NOT NULL THEN ps.Stock ELSE 0 END) AS TotalStock,
    
    -- Estadísticas de precios
    MIN(ps.Price) AS MinPrice,
    MAX(ps.Price) AS MaxPrice,
    AVG(ps.Price) AS AvgPrice,
    
    -- Estadísticas de ventas
    ISNULL(sales.TotalSold, 0) AS TotalSold,
    ISNULL(sales.TotalRevenue, 0) AS TotalRevenue,
    
    -- Estadísticas de wishlist
    ISNULL(wishes.WishlistCount, 0) AS WishlistCount,
    
    c.IsActive,
    c.CreatedAt

FROM CategoryHierarchy ch
INNER JOIN Categories c ON ch.Id = c.Id
LEFT JOIN Products p ON c.Id = p.CategoryId
LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
LEFT JOIN (
    SELECT 
        p.CategoryId,
        SUM(od.Quantity) AS TotalSold,
        SUM(od.TotalPrice) AS TotalRevenue
    FROM OrderDetails od
    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
    INNER JOIN Products p ON ps.ProductId = p.Id
    INNER JOIN Orders o ON od.OrderId = o.Id
    WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    GROUP BY p.CategoryId
) sales ON c.Id = sales.CategoryId
LEFT JOIN (
    SELECT 
        p.CategoryId,
        COUNT(w.Id) AS WishlistCount
    FROM Wishlists w
    INNER JOIN Products p ON w.ProductId = p.Id
    GROUP BY p.CategoryId
) wishes ON c.Id = wishes.CategoryId
WHERE c.IsActive = 1
GROUP BY 
    c.Id, c.Name, c.ParentId, ch.Level, ch.FullPath, c.IsActive, c.CreatedAt,
    sales.TotalSold, sales.TotalRevenue, wishes.WishlistCount;
GO

-- ===============================================
-- 8. VIEW: ESTADÍSTICAS DE USUARIOS
-- ===============================================

CREATE OR ALTER VIEW vw_CustomerStats
AS
SELECT 
    u.Id AS UserId,
    u.FirstName + ' ' + u.LastName AS CustomerName,
    u.Email,
    r.Name AS RoleName,
    
    -- Estadísticas de órdenes
    ISNULL(orders.OrderCount, 0) AS TotalOrders,
    ISNULL(orders.TotalSpent, 0) AS TotalSpent,
    ISNULL(orders.AvgOrderValue, 0) AS AvgOrderValue,
    orders.FirstOrder,
    orders.LastOrder,
    
    -- Estadísticas de wishlist
    ISNULL(wishlist.WishlistCount, 0) AS WishlistCount,
    wishlist.LastWishAdded,
    
    -- Categorías favoritas (más compradas)
    orders.FavoriteCategory,
    
    -- Segmentación de cliente
    CASE 
        WHEN ISNULL(orders.TotalSpent, 0) = 0 THEN 'Nuevo'
        WHEN ISNULL(orders.TotalSpent, 0) < 100 THEN 'Ocasional'
        WHEN ISNULL(orders.TotalSpent, 0) < 500 THEN 'Regular'
        ELSE 'VIP'
    END AS CustomerSegment,
    
    -- Días desde última actividad
    DATEDIFF(DAY, 
        COALESCE(orders.LastOrder, wishlist.LastWishAdded, u.CreatedAt), 
        GETDATE()
    ) AS DaysSinceLastActivity,
    
    u.IsActive,
    u.CreatedAt AS RegistrationDate,
    u.LastLoginAt

FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
LEFT JOIN (
    SELECT 
        o.UserId,
        COUNT(o.Id) AS OrderCount,
        SUM(o.TotalAmount) AS TotalSpent,
        AVG(o.TotalAmount) AS AvgOrderValue,
        MIN(o.OrderDate) AS FirstOrder,
        MAX(o.OrderDate) AS LastOrder,
        
        -- Categoría más comprada
        (SELECT TOP 1 c.Name
         FROM OrderDetails od
         INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
         INNER JOIN Products p ON ps.ProductId = p.Id
         INNER JOIN Categories c ON p.CategoryId = c.Id
         WHERE od.OrderId IN (SELECT Id FROM Orders WHERE UserId = o.UserId)
         GROUP BY c.Name
         ORDER BY SUM(od.Quantity) DESC) AS FavoriteCategory
    FROM Orders o
    WHERE o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    GROUP BY o.UserId
) orders ON u.Id = orders.UserId
LEFT JOIN (
    SELECT 
        UserId,
        COUNT(Id) AS WishlistCount,
        MAX(AddedDate) AS LastWishAdded
    FROM Wishlists
    GROUP BY UserId
) wishlist ON u.Id = wishlist.UserId
WHERE r.Name = 'Customer' AND u.IsActive = 1;
GO

-- ===============================================
-- 9. VIEW: DASHBOARD PRINCIPAL
-- ===============================================

CREATE OR ALTER VIEW vw_DashboardSummary
AS
SELECT 
    -- Resumen de productos
    (SELECT COUNT(*) FROM Products WHERE IsActive = 1) AS TotalProducts,
    (SELECT COUNT(*) FROM vw_ProductsCatalog WHERE IsAvailable = 1) AS AvailableProducts,
    (SELECT COUNT(*) FROM vw_ProductsCatalog WHERE StockStatus = 'Stock Bajo') AS LowStockProducts,
    
    -- Resumen de órdenes
    (SELECT COUNT(*) FROM Orders WHERE Status = 'Pending') AS PendingOrders,
    (SELECT COUNT(*) FROM Orders WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)) AS TodayOrders,
    (SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)) AS TodayRevenue,
    
    -- Resumen de inventario
    (SELECT COUNT(*) FROM Suppliers WHERE IsActive = 1) AS ActiveSuppliers,
    (SELECT ISNULL(SUM(TotalStock), 0) FROM vw_ProductsCatalog) AS TotalStockUnits,
    (SELECT ISNULL(SUM(InventoryValue), 0) FROM vw_DetailedInventory) AS TotalInventoryValue,
    
    -- Resumen de usuarios
    (SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND RoleId = (SELECT Id FROM Roles WHERE Name = 'Customer')) AS TotalCustomers,
    (SELECT COUNT(*) FROM Wishlists) AS TotalWishlistItems,
    
    -- Fecha de última actualización
    GETDATE() AS LastUpdated;
GO

PRINT '';
PRINT '===============================================';
PRINT 'VIEWS CREADAS EXITOSAMENTE';
PRINT '===============================================';
PRINT 'Views implementadas:';
PRINT '- vw_ProductsCatalog: Catálogo con precios agregados';
PRINT '- vw_DetailedInventory: Inventario detallado';
PRINT '- vw_OrdersComplete: Órdenes con detalles completos';
PRINT '- vw_SalesStats: Estadísticas de ventas';
PRINT '- vw_MostWishedProducts: Productos más deseados';
PRINT '- vw_SupplierStats: Estadísticas de proveedores';
PRINT '- vw_CategoryStats: Categorías con estadísticas';
PRINT '- vw_CustomerStats: Estadísticas de clientes';
PRINT '- vw_DashboardSummary: Resumen para dashboard';
PRINT '';
PRINT 'Total: 9 Views especializadas';
PRINT 'PRÓXIMO PASO: Triggers';
PRINT '===============================================';
GO