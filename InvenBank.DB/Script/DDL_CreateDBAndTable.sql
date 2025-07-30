


-- ===============================================
-- SCRIPTS DDL - SISTEMA INVENBANK
-- Base de Datos: SQL Server
-- Propósito: Inventario Multi-Proveedor
-- ===============================================

-- ===============================================
-- 1. CREAR BASE DE DATOS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InvenBank_db')
BEGIN
    CREATE DATABASE InvenBank
    COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO

USE InvenBank_db;
GO

-- ===============================================
-- 2. TABLA: ROLES
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) NOT NULL,
        Name VARCHAR(50) NOT NULL,
        Description VARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Roles_UpdatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_Roles_Id PRIMARY KEY (Id),
        CONSTRAINT UK_Roles_Name UNIQUE (Name)
    );
    
    -- Índices
    CREATE INDEX IX_Roles_Name ON Roles(Name);
    
    PRINT 'Tabla Roles creada exitosamente.';
END
GO

-- ===============================================
-- 3. TABLA: USERS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) NOT NULL,
        Email VARCHAR(255) NOT NULL,
        PasswordHash VARCHAR(255) NOT NULL,
        FirstName VARCHAR(100) NOT NULL,
        LastName VARCHAR(100) NOT NULL,
        RoleId INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT GETDATE(),
        LastLoginAt DATETIME2 NULL,
        
        -- Constraints
        CONSTRAINT PK_Users_Id PRIMARY KEY (Id),
        CONSTRAINT UK_Users_Email UNIQUE (Email),
        CONSTRAINT FK_Users_RoleId FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    );
    
    -- Índices
    CREATE INDEX IX_Users_RoleId ON Users(RoleId);
    CREATE INDEX IX_Users_IsActive ON Users(IsActive) WHERE IsActive = 1;
    CREATE INDEX IX_Users_Email ON Users(Email);
    
    PRINT 'Tabla Users creada exitosamente.';
END
GO

-- ===============================================
-- 4. TABLA: CATEGORIES
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) NOT NULL,
        Name VARCHAR(100) NOT NULL,
        Description VARCHAR(500) NULL,
        ParentId INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Categories_UpdatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_Categories_Id PRIMARY KEY (Id),
        CONSTRAINT FK_Categories_ParentId FOREIGN KEY (ParentId) REFERENCES Categories(Id)
    );
    
    -- Índices
    CREATE INDEX IX_Categories_ParentId ON Categories(ParentId);
    CREATE INDEX IX_Categories_IsActive ON Categories(IsActive) WHERE IsActive = 1;
    CREATE INDEX IX_Categories_Name ON Categories(Name);
    
    PRINT 'Tabla Categories creada exitosamente.';
END
GO

-- ===============================================
-- 5. TABLA: PRODUCTS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) NOT NULL,
        Name VARCHAR(200) NOT NULL,
        Description TEXT NULL,
        SKU VARCHAR(50) NOT NULL,
        CategoryId INT NOT NULL,
        ImageUrl VARCHAR(500) NULL,
        Brand VARCHAR(100) NULL,
        Specifications TEXT NULL, -- JSON format
        IsActive BIT NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Products_UpdatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_Products_Id PRIMARY KEY (Id),
        CONSTRAINT UK_Products_SKU UNIQUE (SKU),
        CONSTRAINT FK_Products_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
    );
    
    -- Índices
    CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
    CREATE INDEX IX_Products_IsActive ON Products(IsActive) WHERE IsActive = 1;
    CREATE INDEX IX_Products_Name_Brand ON Products(Name, Brand);
    CREATE INDEX IX_Products_SKU ON Products(SKU);
    
    PRINT 'Tabla Products creada exitosamente.';
END
GO

-- ===============================================
-- 6. TABLA: SUPPLIERS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Suppliers')
BEGIN
    CREATE TABLE Suppliers (
        Id INT IDENTITY(1,1) NOT NULL,
        Name VARCHAR(200) NOT NULL,
        ContactPerson VARCHAR(150) NULL,
        Email VARCHAR(255) NULL,
        Phone VARCHAR(20) NULL,
        Address TEXT NULL,
        TaxId VARCHAR(50) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Suppliers_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Suppliers_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Suppliers_UpdatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_Suppliers_Id PRIMARY KEY (Id)
    );
    
    -- Índices
    CREATE INDEX IX_Suppliers_IsActive ON Suppliers(IsActive) WHERE IsActive = 1;
    CREATE INDEX IX_Suppliers_Name ON Suppliers(Name);
    CREATE UNIQUE INDEX IX_Suppliers_TaxId ON Suppliers(TaxId) WHERE TaxId IS NOT NULL;
    
    PRINT 'Tabla Suppliers creada exitosamente.';
END
GO

-- ===============================================
-- 7. TABLA: PRODUCTSUPPLIERS (TABLA CENTRAL)
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductSuppliers')
BEGIN
    CREATE TABLE ProductSuppliers (
        Id INT IDENTITY(1,1) NOT NULL,
        ProductId INT NOT NULL,
        SupplierId INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL,
        BatchNumber VARCHAR(50) NULL,
        SupplierSKU VARCHAR(50) NULL,
        LastRestockDate DATETIME2 NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ProductSuppliers_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductSuppliers_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductSuppliers_UpdatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_ProductSuppliers_Id PRIMARY KEY (Id),
        CONSTRAINT UK_ProductSuppliers_ProductId_SupplierId UNIQUE (ProductId, SupplierId),
        CONSTRAINT FK_ProductSuppliers_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id),
        CONSTRAINT FK_ProductSuppliers_SupplierId FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
        CONSTRAINT CK_ProductSuppliers_Price_Positive CHECK (Price > 0),
        CONSTRAINT CK_ProductSuppliers_Stock_NonNegative CHECK (Stock >= 0)
    );
    
    -- Índices
    CREATE INDEX IX_ProductSuppliers_ProductId ON ProductSuppliers(ProductId);
    CREATE INDEX IX_ProductSuppliers_SupplierId ON ProductSuppliers(SupplierId);
    CREATE INDEX IX_ProductSuppliers_Price ON ProductSuppliers(Price);
    CREATE INDEX IX_ProductSuppliers_Stock ON ProductSuppliers(Stock) WHERE Stock > 0;
    CREATE INDEX IX_ProductSuppliers_IsActive ON ProductSuppliers(IsActive) WHERE IsActive = 1;
    
    PRINT 'Tabla ProductSuppliers creada exitosamente.';
END
GO

-- ===============================================
-- 8. TABLA: ORDERS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) NOT NULL,
        OrderNumber VARCHAR(20) NOT NULL,
        UserId INT NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        Status VARCHAR(20) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT 'Pending',
        OrderDate DATETIME2 NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT GETDATE(),
        DeliveryDate DATETIME2 NULL,
        ShippingAddress TEXT NOT NULL,
        Notes TEXT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Orders_UpdatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_Orders_Id PRIMARY KEY (Id),
        CONSTRAINT UK_Orders_OrderNumber UNIQUE (OrderNumber),
        CONSTRAINT FK_Orders_UserId FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT CK_Orders_TotalAmount_NonNegative CHECK (TotalAmount >= 0),
        CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'))
    );
    
    -- Índices
    CREATE INDEX IX_Orders_UserId ON Orders(UserId);
    CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
    CREATE INDEX IX_Orders_Status ON Orders(Status);
    CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
    
    PRINT 'Tabla Orders creada exitosamente.';
END
GO

-- ===============================================
-- 9. TABLA: ORDERDETAILS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderDetails')
BEGIN
    CREATE TABLE OrderDetails (
        Id INT IDENTITY(1,1) NOT NULL,
        OrderId INT NOT NULL,
        ProductSupplierId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalPrice AS (Quantity * UnitPrice) PERSISTED, -- Columna calculada
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_OrderDetails_CreatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_OrderDetails_Id PRIMARY KEY (Id),
        CONSTRAINT FK_OrderDetails_OrderId FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderDetails_ProductSupplierId FOREIGN KEY (ProductSupplierId) REFERENCES ProductSuppliers(Id),
        CONSTRAINT CK_OrderDetails_Quantity_Positive CHECK (Quantity > 0),
        CONSTRAINT CK_OrderDetails_UnitPrice_Positive CHECK (UnitPrice > 0)
    );
    
    -- Índices
    CREATE INDEX IX_OrderDetails_OrderId ON OrderDetails(OrderId);
    CREATE INDEX IX_OrderDetails_ProductSupplierId ON OrderDetails(ProductSupplierId);
    
    PRINT 'Tabla OrderDetails creada exitosamente.';
END
GO

-- ===============================================
-- 10. TABLA: WISHLISTS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wishlists')
BEGIN
    CREATE TABLE Wishlists (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        ProductId INT NOT NULL,
        AddedDate DATETIME2 NOT NULL CONSTRAINT DF_Wishlists_AddedDate DEFAULT GETDATE(),
        Notes TEXT NULL,
        
        -- Constraints
        CONSTRAINT PK_Wishlists_Id PRIMARY KEY (Id),
        CONSTRAINT UK_Wishlists_UserId_ProductId UNIQUE (UserId, ProductId),
        CONSTRAINT FK_Wishlists_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Wishlists_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );
    
    -- Índices
    CREATE INDEX IX_Wishlists_UserId ON Wishlists(UserId);
    CREATE INDEX IX_Wishlists_ProductId ON Wishlists(ProductId);
    CREATE INDEX IX_Wishlists_AddedDate ON Wishlists(AddedDate);
    
    PRINT 'Tabla Wishlists creada exitosamente.';
END
GO

-- ===============================================
-- 11. TABLA: AUDITLOGS
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id BIGINT IDENTITY(1,1) NOT NULL,
        TableName VARCHAR(100) NOT NULL,
        Operation VARCHAR(10) NOT NULL,
        RecordId INT NOT NULL,
        UserId INT NULL,
        OldValues TEXT NULL, -- JSON format
        NewValues TEXT NULL, -- JSON format
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT PK_AuditLogs_Id PRIMARY KEY (Id),
        CONSTRAINT FK_AuditLogs_UserId FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT CK_AuditLogs_Operation CHECK (Operation IN ('INSERT', 'UPDATE', 'DELETE'))
    );
    
    -- Índices
    CREATE INDEX IX_AuditLogs_TableName_RecordId ON AuditLogs(TableName, RecordId);
    CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
    CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
    CREATE INDEX IX_AuditLogs_Operation ON AuditLogs(Operation);
    
    PRINT 'Tabla AuditLogs creada exitosamente.';
END
GO

-- ===============================================
-- 12. DATOS INICIALES
-- ===============================================

-- Insertar Roles
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Name, Description) VALUES 
    ('Admin', 'Administrador del sistema con acceso completo'),
    ('Customer', 'Cliente con acceso a compras y wishlist');
    
    PRINT 'Roles insertados exitosamente.';
END
GO

-- Insertar Categorías Base
IF NOT EXISTS (SELECT * FROM Categories WHERE Name = 'Electrónicos')
BEGIN
    INSERT INTO Categories (Name, Description, ParentId) VALUES 
    ('Electrónicos', 'Productos electrónicos y tecnológicos', NULL),
    ('Hogar', 'Productos para el hogar', NULL),
    ('Ropa', 'Ropa y accesorios', NULL);
    
    -- Subcategorías
    DECLARE @ElectronicosId INT = (SELECT Id FROM Categories WHERE Name = 'Electrónicos');
    
    INSERT INTO Categories (Name, Description, ParentId) VALUES 
    ('Monitores', 'Monitores y pantallas', @ElectronicosId),
    ('Audio', 'Equipos de sonido y audio', @ElectronicosId),
    ('Computadoras', 'Laptops y desktops', @ElectronicosId);
    
    PRINT 'Categorías base insertadas exitosamente.';
END
GO

-- Insertar Usuario Administrador por defecto
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@invenbank.com')
BEGIN
    DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');
    
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleId) VALUES 
    ('admin@invenbank.com', 'AQAAAAIAAYagAAAAEJhOJf8iFqNLrR1ZOCL3nkVy4nj+kBvHHO6Mf+5x9YvCdNa6lD9nXzPJ4QzGhC8VQw==', 'Admin', 'Sistema', @AdminRoleId);
    
    PRINT 'Usuario administrador creado exitosamente.';
    PRINT 'Email: admin@invenbank.com';
    PRINT 'Password: Admin123!';
END
GO

-- Insertar Proveedores de ejemplo
IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'TechSupply Corp')
BEGIN
    INSERT INTO Suppliers (Name, ContactPerson, Email, Phone, Address, TaxId) VALUES 
    ('TechSupply Corp', 'Juan Pérez', 'ventas@techsupply.com', '+507-6000-1234', 'Zona Libre de Colón, Panamá', 'RUC-001-123456-1-DV'),
    ('ElectroMax SA', 'María González', 'info@electromax.com', '+507-6000-5678', 'Vía España, Ciudad de Panamá', 'RUC-002-789012-2-DV'),
    ('Digital World', 'Carlos Rodríguez', 'compras@digitalworld.com', '+507-6000-9012', 'Albrook Mall, Panamá', 'RUC-003-345678-3-DV');
    
    PRINT 'Proveedores de ejemplo insertados exitosamente.';
END
GO

PRINT '';
PRINT '===============================================';
PRINT 'BASE DE DATOS INVENBANK CREADA EXITOSAMENTE';
PRINT '===============================================';
PRINT 'Tablas creadas: 11';
PRINT 'Índices creados: 40+';
PRINT 'Constraints creados: 25+';
PRINT 'Datos iniciales: Roles, Categorías, Admin, Proveedores';
PRINT '';
PRINT 'PRÓXIMO PASO: Stored Procedures';
PRINT '===============================================';
GO