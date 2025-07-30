-- ===============================================
-- STORED PROCEDURES - SISTEMA INVENBANK
-- Propósito: CRUD optimizado y lógica de negocio
-- ===============================================

USE InvenBank_db;
GO

-- ===============================================
-- 1. MÓDULO: USUARIOS Y AUTENTICACIÓN
-- ===============================================

-- SP: Crear Usuario
CREATE OR ALTER PROCEDURE sp_CreateUser
    @Email VARCHAR(255),
    @PasswordHash VARCHAR(255),
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @RoleName VARCHAR(50) = 'Customer'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el email no exista
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            THROW 50001, 'El email ya está registrado en el sistema.', 1;
        END
        
        -- Obtener RoleId
        DECLARE @RoleId INT;
        SELECT @RoleId = Id FROM Roles WHERE Name = @RoleName;
        
        IF @RoleId IS NULL
        BEGIN
            THROW 50002, 'El rol especificado no existe.', 1;
        END
        
        -- Insertar usuario
        INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleId)
        VALUES (@Email, @PasswordHash, @FirstName, @LastName, @RoleId);
        
        -- Retornar usuario creado
        SELECT 
            Id, Email, FirstName, LastName, 
            (SELECT Name FROM Roles WHERE Id = RoleId) AS RoleName,
            IsActive, CreatedAt
        FROM Users 
        WHERE Id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Autenticar Usuario
CREATE OR ALTER PROCEDURE sp_AuthenticateUser
    @Email VARCHAR(255),
    @PasswordHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId INT;
    
    -- Verificar credenciales
    SELECT @UserId = Id 
    FROM Users 
    WHERE Email = @Email 
      AND PasswordHash = @PasswordHash 
      AND IsActive = 1;
    
    IF @UserId IS NOT NULL
    BEGIN
        -- Actualizar último login
        UPDATE Users 
        SET LastLoginAt = GETDATE() 
        WHERE Id = @UserId;
        
        -- Retornar datos del usuario
        SELECT 
            u.Id, u.Email, u.FirstName, u.LastName,
            r.Name AS RoleName, u.LastLoginAt
        FROM Users u
        INNER JOIN Roles r ON u.RoleId = r.Id
        WHERE u.Id = @UserId;
    END
    ELSE
    BEGIN
        -- Credenciales inválidas
        SELECT NULL AS Id;
    END
END
GO

-- ===============================================
-- 2. MÓDULO: PRODUCTOS
-- ===============================================

-- SP: Crear Producto
CREATE OR ALTER PROCEDURE sp_CreateProduct
    @Name VARCHAR(200),
    @Description TEXT,
    @SKU VARCHAR(50),
    @CategoryId INT,
    @ImageUrl VARCHAR(500) = NULL,
    @Brand VARCHAR(100) = NULL,
    @Specifications TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el SKU no exista
        IF EXISTS (SELECT 1 FROM Products WHERE SKU = @SKU)
        BEGIN
            THROW 50003, 'El SKU ya existe en el sistema.', 1;
        END
        
        -- Validar que la categoría exista
        IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @CategoryId AND IsActive = 1)
        BEGIN
            THROW 50004, 'La categoría especificada no existe o está inactiva.', 1;
        END
        
        -- Insertar producto
        INSERT INTO Products (Name, Description, SKU, CategoryId, ImageUrl, Brand, Specifications)
        VALUES (@Name, @Description, @SKU, @CategoryId, @ImageUrl, @Brand, @Specifications);
        
        -- Retornar producto creado
        SELECT 
            p.Id, p.Name, p.Description, p.SKU, p.Brand,
            c.Name AS CategoryName, p.ImageUrl, p.Specifications,
            p.IsActive, p.CreatedAt
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE p.Id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Buscar Productos (con filtros)
CREATE OR ALTER PROCEDURE sp_SearchProducts
    @SearchTerm VARCHAR(200) = NULL,
    @CategoryId INT = NULL,
    @Brand VARCHAR(100) = NULL,
    @MinPrice DECIMAL(18,2) = NULL,
    @MaxPrice DECIMAL(18,2) = NULL,
    @InStock BIT = 1,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH ProductsWithPricing AS (
        SELECT 
            p.Id, p.Name, CAST(p.Description AS VARCHAR(MAX)) AS Description, p.SKU, p.Brand, p.ImageUrl,
            c.Name AS CategoryName,
            MIN(ps.Price) AS MinPrice,
            MAX(ps.Price) AS MaxPrice,
            SUM(ps.Stock) AS TotalStock,
            COUNT(ps.Id) AS SupplierCount
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
        WHERE p.IsActive = 1
        GROUP BY p.Id, p.Name, CAST(p.Description AS VARCHAR(MAX)), p.SKU, p.Brand, p.ImageUrl, c.Name
    )
    SELECT 
        Id, Name, Description, SKU, Brand, ImageUrl, CategoryName,
        ISNULL(MinPrice, 0) AS MinPrice,
        ISNULL(MaxPrice, 0) AS MaxPrice,
        ISNULL(TotalStock, 0) AS TotalStock,
        ISNULL(SupplierCount, 0) AS SupplierCount
    FROM ProductsWithPricing
    WHERE (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%' OR CAST(Description AS VARCHAR(MAX)) LIKE '%' + @SearchTerm + '%')
      AND (@CategoryId IS NULL OR CategoryName IN (SELECT Name FROM Categories WHERE Id = @CategoryId OR ParentId = @CategoryId))
      AND (@Brand IS NULL OR Brand = @Brand)
      AND (@MinPrice IS NULL OR MinPrice >= @MinPrice)
      AND (@MaxPrice IS NULL OR MaxPrice <= @MaxPrice)
      AND (@InStock = 0 OR TotalStock > 0)
    ORDER BY Name
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Retornar total de registros para paginación
    SELECT COUNT(*) AS TotalRecords
    FROM ProductsWithPricing
    WHERE (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%' OR CAST(Description AS VARCHAR(MAX)) LIKE '%' + @SearchTerm + '%')
      AND (@CategoryId IS NULL OR CategoryName IN (SELECT Name FROM Categories WHERE Id = @CategoryId OR ParentId = @CategoryId))
      AND (@Brand IS NULL OR Brand = @Brand)
      AND (@MinPrice IS NULL OR MinPrice >= @MinPrice)
      AND (@MaxPrice IS NULL OR MaxPrice <= @MaxPrice)
      AND (@InStock = 0 OR TotalStock > 0);
END
GO

-- SP: Obtener Detalle de Producto con Proveedores
CREATE OR ALTER PROCEDURE sp_GetProductDetails
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Información del producto
    SELECT 
        p.Id, p.Name, p.Description, p.SKU, p.Brand, p.ImageUrl, p.Specifications,
        c.Name AS CategoryName, c.Id AS CategoryId,
        p.IsActive, p.CreatedAt, p.UpdatedAt
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    WHERE p.Id = @ProductId;
    
    -- Proveedores y precios
    SELECT 
        ps.Id AS ProductSupplierId,
        s.Id AS SupplierId, s.Name AS SupplierName,
        ps.Price, ps.Stock, ps.BatchNumber, ps.SupplierSKU,
        ps.LastRestockDate, ps.IsActive
    FROM ProductSuppliers ps
    INNER JOIN Suppliers s ON ps.SupplierId = s.Id
    WHERE ps.ProductId = @ProductId AND ps.IsActive = 1
    ORDER BY ps.Price ASC;
END
GO

-- ===============================================
-- 3. MÓDULO: PRODUCTO-PROVEEDORES (CORE)
-- ===============================================

-- SP: Asociar Producto con Proveedor
CREATE OR ALTER PROCEDURE sp_CreateProductSupplier
    @ProductId INT,
    @SupplierId INT,
    @Price DECIMAL(18,2),
    @Stock INT,
    @BatchNumber VARCHAR(50) = NULL,
    @SupplierSKU VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el producto exista
        IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @ProductId AND IsActive = 1)
        BEGIN
            THROW 50005, 'El producto especificado no existe o está inactivo.', 1;
        END
        
        -- Validar que el proveedor exista
        IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = @SupplierId AND IsActive = 1)
        BEGIN
            THROW 50006, 'El proveedor especificado no existe o está inactivo.', 1;
        END
        
        -- Validar que no exista la combinación
        IF EXISTS (SELECT 1 FROM ProductSuppliers WHERE ProductId = @ProductId AND SupplierId = @SupplierId)
        BEGIN
            THROW 50007, 'Ya existe una asociación entre este producto y proveedor.', 1;
        END
        
        -- Insertar asociación
        INSERT INTO ProductSuppliers (ProductId, SupplierId, Price, Stock, BatchNumber, SupplierSKU)
        VALUES (@ProductId, @SupplierId, @Price, @Stock, @BatchNumber, @SupplierSKU);
        
        -- Retornar asociación creada
        SELECT 
            ps.Id, p.Name AS ProductName, s.Name AS SupplierName,
            ps.Price, ps.Stock, ps.BatchNumber, ps.SupplierSKU,
            ps.IsActive, ps.CreatedAt
        FROM ProductSuppliers ps
        INNER JOIN Products p ON ps.ProductId = p.Id
        INNER JOIN Suppliers s ON ps.SupplierId = s.Id
        WHERE ps.Id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Actualizar Stock
CREATE OR ALTER PROCEDURE sp_UpdateStock
    @ProductSupplierId INT,
    @NewStock INT,
    @Reason VARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @OldStock INT, @ProductId INT, @SupplierId INT;
        
        -- Obtener stock actual
        SELECT @OldStock = Stock, @ProductId = ProductId, @SupplierId = SupplierId
        FROM ProductSuppliers 
        WHERE Id = @ProductSupplierId AND IsActive = 1;
        
        IF @OldStock IS NULL
        BEGIN
            THROW 50008, 'El registro de producto-proveedor no existe o está inactivo.', 1;
        END
        
        -- Actualizar stock
        UPDATE ProductSuppliers 
        SET Stock = @NewStock, 
            UpdatedAt = GETDATE(),
            LastRestockDate = CASE WHEN @NewStock > @OldStock THEN GETDATE() ELSE LastRestockDate END
        WHERE Id = @ProductSupplierId;
        
        -- Log de auditoría (opcional)
        INSERT INTO AuditLogs (TableName, Operation, RecordId, OldValues, NewValues)
        VALUES ('ProductSuppliers', 'UPDATE', @ProductSupplierId, 
               (SELECT @OldStock AS Stock FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
               (SELECT @NewStock AS Stock, @Reason AS Reason FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
                                );

        
        -- Retornar resultado
        SELECT 
            'Stock actualizado exitosamente' AS Message,
            @OldStock AS OldStock,
            @NewStock AS NewStock,
            (@NewStock - @OldStock) AS StockDifference
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ===============================================
-- 4. MÓDULO: ÓRDENES Y COMPRAS
-- ===============================================

-- SP: Crear Orden
CREATE OR ALTER PROCEDURE sp_CreateOrder
    @UserId INT,
    @ShippingAddress TEXT,
    @OrderItems NVARCHAR(MAX), -- JSON array: [{"ProductSupplierId": 1, "Quantity": 2}, ...]
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar usuario
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @UserId AND IsActive = 1)
        BEGIN
            THROW 50009, 'El usuario especificado no existe o está inactivo.', 1;
        END
        
        -- Generar número de orden
        DECLARE @OrderNumber VARCHAR(20);
        DECLARE @OrderDate DATE = CAST(GETDATE() AS DATE);
        DECLARE @OrderCount INT;
        
        SELECT @OrderCount = COUNT(*) + 1
        FROM Orders 
        WHERE CAST(OrderDate AS DATE) = @OrderDate;
        
        SET @OrderNumber = 'ORD-' + FORMAT(@OrderDate, 'yyyyMMdd') + '-' + FORMAT(@OrderCount, '0000');
        
        -- Validar items y calcular total
        DECLARE @TotalAmount DECIMAL(18,2) = 0;
        DECLARE @OrderId INT;
        
        -- Crear tabla temporal para items
        CREATE TABLE #OrderItems (
            ProductSupplierId INT,
            Quantity INT,
            UnitPrice DECIMAL(18,2),
            AvailableStock INT
        );
        
        -- Parsear JSON e insertar en tabla temporal
        INSERT INTO #OrderItems (ProductSupplierId, Quantity, UnitPrice, AvailableStock)
        SELECT 
            JSON_VALUE(value, '$.ProductSupplierId') AS ProductSupplierId,
            JSON_VALUE(value, '$.Quantity') AS Quantity,
            ps.Price AS UnitPrice,
            ps.Stock AS AvailableStock
        FROM OPENJSON(@OrderItems) 
        CROSS JOIN ProductSuppliers ps
        WHERE ps.Id = JSON_VALUE(value, '$.ProductSupplierId') AND ps.IsActive = 1;
        
        -- Validar stock disponible
        IF EXISTS (SELECT 1 FROM #OrderItems WHERE Quantity > AvailableStock)
        BEGIN
            THROW 50010, 'Stock insuficiente para uno o más productos.', 1;
        END
        
        -- Calcular total
        SELECT @TotalAmount = SUM(Quantity * UnitPrice) FROM #OrderItems;
        
        -- Crear orden
        INSERT INTO Orders (OrderNumber, UserId, TotalAmount, ShippingAddress, Notes)
        VALUES (@OrderNumber, @UserId, @TotalAmount, @ShippingAddress, @Notes);
        
        SET @OrderId = SCOPE_IDENTITY();
        
        -- Crear detalles de orden
        INSERT INTO OrderDetails (OrderId, ProductSupplierId, Quantity, UnitPrice)
        SELECT @OrderId, ProductSupplierId, Quantity, UnitPrice
        FROM #OrderItems;
        
        -- Reducir stock
        UPDATE ps
        SET Stock = ps.Stock - oi.Quantity,
            UpdatedAt = GETDATE()
        FROM ProductSuppliers ps
        INNER JOIN #OrderItems oi ON ps.Id = oi.ProductSupplierId;
        
        -- Retornar orden creada
        SELECT 
            o.Id, o.OrderNumber, o.TotalAmount, o.Status,
            o.OrderDate, o.ShippingAddress, o.Notes
        FROM Orders o
        WHERE o.Id = @OrderId;
        
        DROP TABLE #OrderItems;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF OBJECT_ID('tempdb..#OrderItems') IS NOT NULL
            DROP TABLE #OrderItems;
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Obtener Órdenes de Usuario
CREATE OR ALTER PROCEDURE sp_GetUserOrders
    @UserId INT,
    @Status VARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        o.Id, o.OrderNumber, o.TotalAmount, o.Status,
        o.OrderDate, o.DeliveryDate, CAST(o.ShippingAddress AS varchar(MAX)),
        COUNT(od.Id) AS ItemCount
    FROM Orders o
    LEFT JOIN OrderDetails od ON o.Id = od.OrderId
    WHERE o.UserId = @UserId
      AND (@Status IS NULL OR o.Status = @Status)
    GROUP BY o.Id, o.OrderNumber, o.TotalAmount, o.Status, 
             o.OrderDate, o.DeliveryDate, CAST(o.ShippingAddress AS varchar(MAX))
    ORDER BY o.OrderDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ===============================================
-- 5. MÓDULO: LISTA DE DESEOS
-- ===============================================

-- SP: Agregar a Wishlist
CREATE OR ALTER PROCEDURE sp_AddToWishlist
    @UserId INT,
    @ProductId INT,
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificar si ya existe
        IF EXISTS (SELECT 1 FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId)
        BEGIN
            SELECT 'El producto ya está en tu lista de deseos' AS Message;
            RETURN;
        END
        
        -- Agregar a wishlist
        INSERT INTO Wishlists (UserId, ProductId, Notes)
        VALUES (@UserId, @ProductId, @Notes);
        
        -- Retornar producto agregado
        SELECT 
            w.Id, p.Name AS ProductName, p.SKU, p.ImageUrl,
            w.AddedDate, w.Notes
        FROM Wishlists w
        INNER JOIN Products p ON w.ProductId = p.Id
        WHERE w.Id = SCOPE_IDENTITY();
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- SP: Obtener Wishlist de Usuario
CREATE OR ALTER PROCEDURE sp_GetUserWishlist
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        w.Id AS WishlistId, w.AddedDate, CAST(w.Notes AS VARCHAR(MAX)) AS Notes,
        p.Id AS ProductId, p.Name, p.SKU, p.ImageUrl, p.Brand,
        c.Name AS CategoryName,
        MIN(ps.Price) AS MinPrice,
        SUM(ps.Stock) AS TotalStock
    FROM Wishlists w
    INNER JOIN Products p ON w.ProductId = p.Id
    INNER JOIN Categories c ON p.CategoryId = c.Id
    LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId AND ps.IsActive = 1
    WHERE w.UserId = @UserId
    GROUP BY w.Id, w.AddedDate, CAST(w.Notes AS varchar(MAX)), p.Id, p.Name, p.SKU, p.ImageUrl, p.Brand, c.Name
    ORDER BY w.AddedDate DESC;
END
GO

-- ===============================================
-- 6. REPORTES Y CONSULTAS ESPECIALIZADAS
-- ===============================================

-- SP: Reporte de Inventario
CREATE OR ALTER PROCEDURE sp_InventoryReport
    @CategoryId INT = NULL,
    @SupplierId INT = NULL,
    @LowStockThreshold INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id AS ProductId, p.Name AS ProductName, p.SKU,
        c.Name AS CategoryName,
        s.Id AS SupplierId, s.Name AS SupplierName,
        ps.Price, ps.Stock, ps.BatchNumber,
        CASE 
            WHEN ps.Stock = 0 THEN 'Sin Stock'
            WHEN ps.Stock <= @LowStockThreshold THEN 'Stock Bajo'
            ELSE 'Normal'
        END AS StockStatus,
        ps.LastRestockDate
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    LEFT JOIN ProductSuppliers ps ON p.Id = ps.ProductId
    LEFT JOIN Suppliers s ON ps.SupplierId = s.Id
    WHERE p.IsActive = 1
      AND (@CategoryId IS NULL OR c.Id = @CategoryId OR c.ParentId = @CategoryId)
      AND (@SupplierId IS NULL OR s.Id = @SupplierId)
      AND ps.IsActive = 1
    ORDER BY p.Name, s.Name;
END
GO

-- SP: Productos Más Vendidos
CREATE OR ALTER PROCEDURE sp_TopSellingProducts
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @TopCount INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @StartDate IS NULL SET @StartDate = DATEADD(MONTH, -1, GETDATE());
    IF @EndDate IS NULL SET @EndDate = GETDATE();
    
    SELECT TOP (@TopCount)
        p.Id, p.Name, p.SKU, p.Brand,
        SUM(od.Quantity) AS TotalSold,
        SUM(od.TotalPrice) AS TotalRevenue,
        AVG(od.UnitPrice) AS AvgPrice,
        COUNT(DISTINCT o.UserId) AS UniqueCustomers
    FROM OrderDetails od
    INNER JOIN Orders o ON od.OrderId = o.Id
    INNER JOIN ProductSuppliers ps ON od.ProductSupplierId = ps.Id
    INNER JOIN Products p ON ps.ProductId = p.Id
    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
      AND o.Status IN ('Confirmed', 'Shipped', 'Delivered')
    GROUP BY p.Id, p.Name, p.SKU, p.Brand
    ORDER BY TotalSold DESC;
END
GO

PRINT '';
PRINT '===============================================';
PRINT 'STORED PROCEDURES CREADOS EXITOSAMENTE';
PRINT '===============================================';
PRINT 'Módulos implementados:';
PRINT '- Usuarios y Autenticación (2 SPs)';
PRINT '- Productos (3 SPs)';
PRINT '- Producto-Proveedores (2 SPs)';
PRINT '- Órdenes y Compras (2 SPs)';
PRINT '- Lista de Deseos (2 SPs)';
PRINT '- Reportes y Consultas (2 SPs)';
PRINT '';
PRINT 'Total: 13 Stored Procedures';
PRINT 'PRÓXIMO PASO: Views';
PRINT '===============================================';
GO