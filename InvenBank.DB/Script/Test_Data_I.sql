-- ===============================================
-- DATOS DE PRUEBA - SISTEMA INVENBANK
-- Propósito: Datos realistas para testing completo
-- ===============================================

USE InvenBank_db;
GO

PRINT '===============================================';
PRINT 'INSERTANDO DATOS DE PRUEBA - INVENBANK';
PRINT '===============================================';

-- ===============================================
-- 1. USUARIOS DE PRUEBA
-- ===============================================

-- Clientes de prueba
DECLARE @CustomerRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Customer');

INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleId) VALUES 
('juan.perez@email.com', 'AQAAAAIAAYagAAAAEJhOJf8iFqNLrR1ZOCL3nkVy4nj+kBvHHO6Mf+5x9YvCdNa6lD9nXzPJ4QzGhC8VQw==', 'Juan', 'Pérez', @CustomerRoleId),
('maria.gonzalez@email.com', 'AQAAAAIAAYagAAAAEJhOJf8iFqNLrR1ZOCL3nkVy4nj+kBvHHO6Mf+5x9YvCdNa6lD9nXzPJ4QzGhC8VQw==', 'María', 'González', @CustomerRoleId),
('carlos.rodriguez@email.com', 'AQAAAAIAAYagAAAAEJhOJf8iFqNLrR1ZOCL3nkVy4nj+kBvHHO6Mf+5x9YvCdNa6lD9nXzPJ4QzGhC8VQw==', 'Carlos', 'Rodríguez', @CustomerRoleId),
('ana.martinez@email.com', 'AQAAAAIAAYagAAAAEJhOJf8iFqNLrR1ZOCL3nkVy4nj+kBvHHO6Mf+5x9YvCdNa6lD9nXzPJ4QzGhC8VQw==', 'Ana', 'Martínez', @CustomerRoleId),
('luis.torres@email.com', 'AQAAAAIAAYagAAAAEJhOJf8iFqNLrR1ZOCL3nkVy4nj+kBvHHO6Mf+5x9YvCdNa6lD9nXzPJ4QzGhC8VQw==', 'Luis', 'Torres', @CustomerRoleId);

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'sistema@invenbank.local')
BEGIN
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleId, IsActive)
    SELECT 
        'sistema@invenbank.local',
        'N/A',
        'Sistema',
        'Automático',
        (SELECT TOP 1 Id FROM Roles WHERE Name = 'Admin'),
        1;
END
GO

PRINT '✅ Usuarios de prueba insertados: 5 clientes';

-- ===============================================
-- 2. CATEGORÍAS COMPLETAS
-- ===============================================

-- Obtener IDs de categorías principales
DECLARE @ElectronicosId INT = (SELECT Id FROM Categories WHERE Name = 'Electrónicos');
DECLARE @HogarId INT = (SELECT Id FROM Categories WHERE Name = 'Hogar');
DECLARE @RopaId INT = (SELECT Id FROM Categories WHERE Name = 'Ropa');

-- Subcategorías adicionales
INSERT INTO Categories (Name, Description, ParentId) VALUES 
-- Electrónicos
('Smartphones', 'Teléfonos inteligentes y accesorios', @ElectronicosId),
('Tablets', 'Tabletas y accesorios', @ElectronicosId),
('Gaming', 'Consolas y videojuegos', @ElectronicosId),
('Fotografía', 'Cámaras y equipos fotográficos', @ElectronicosId),

-- Hogar
('Cocina', 'Electrodomésticos de cocina', @HogarId),
('Limpieza', 'Productos de limpieza', @HogarId),
('Decoración', 'Artículos decorativos', @HogarId),

-- Ropa
('Hombre', 'Ropa para hombre', @RopaId),
('Mujer', 'Ropa para mujer', @RopaId),
('Accesorios', 'Accesorios de moda', @RopaId);

PRINT '✅ Categorías expandidas: 13 categorías totales';

-- ===============================================
-- 3. PRODUCTOS REALISTAS
-- ===============================================

-- Obtener IDs de categorías específicas
DECLARE @MonitoresId INT = (SELECT Id FROM Categories WHERE Name = 'Monitores');
DECLARE @AudioId INT = (SELECT Id FROM Categories WHERE Name = 'Audio');
DECLARE @ComputadorasId INT = (SELECT Id FROM Categories WHERE Name = 'Computadoras');
DECLARE @SmartphonesId INT = (SELECT Id FROM Categories WHERE Name = 'Smartphones');
DECLARE @TabletsId INT = (SELECT Id FROM Categories WHERE Name = 'Tablets');
DECLARE @GamingId INT = (SELECT Id FROM Categories WHERE Name = 'Gaming');
DECLARE @FotografiaId INT = (SELECT Id FROM Categories WHERE Name = 'Fotografía');
DECLARE @CocinaId INT = (SELECT Id FROM Categories WHERE Name = 'Cocina');
DECLARE @AccesoríosId INT = (SELECT Id FROM Categories WHERE Name = 'Accesorios');

-- Productos de Electrónicos
INSERT INTO Products (Name, Description, SKU, CategoryId, ImageUrl, Brand, Specifications) VALUES 
-- Monitores
('Monitor LG 27 4K UltraHD', 'Monitor profesional 27 pulgadas con resolución 4K, panel IPS y HDR10', 'MON-LG27-4K-001', @MonitoresId, '/images/monitor-lg-27-4k.jpg', 'LG', '{"resolution": "3840x2160", "panel": "IPS", "refresh_rate": "60Hz", "hdr": true}'),
('Monitor Samsung 32 Curvo Gaming', 'Monitor gaming curvo de 32 pulgadas, 144Hz, 1ms de respuesta', 'MON-SAM32-GAM-002', @MonitoresId, '/images/monitor-samsung-32-gaming.jpg', 'Samsung', '{"resolution": "2560x1440", "panel": "VA", "refresh_rate": "144Hz", "curved": true}'),
('Monitor ASUS 24 Full HD', 'Monitor ASUS de 24 pulgadas Full HD para oficina', 'MON-ASU24-FHD-003', @MonitoresId, '/images/monitor-asus-24-fhd.jpg', 'ASUS', '{"resolution": "1920x1080", "panel": "TN", "refresh_rate": "75Hz", "vesa": true}'),

-- Audio
('Auriculares Sony WH-1000XM5', 'Auriculares inalámbricos con cancelación de ruido premium', 'AUD-SON-WH1000-004', @AudioId, '/images/sony-wh1000xm5.jpg', 'Sony', '{"wireless": true, "noise_cancellation": true, "battery": "30h", "bluetooth": "5.2"}'),
('Parlante JBL Flip 6', 'Parlante portátil resistente al agua con sonido JBL Pro', 'AUD-JBL-FLIP6-005', @AudioId, '/images/jbl-flip6.jpg', 'JBL', '{"waterproof": "IP67", "battery": "12h", "wireless": true, "power": "20W"}'),
('Micrófono Blue Yeti', 'Micrófono profesional USB para streaming y podcasting', 'AUD-BLU-YETI-006', @AudioId, '/images/blue-yeti.jpg', 'Blue', '{"connection": "USB", "pattern": "multiple", "professional": true, "stand": true}'),

-- Computadoras
('MacBook Air M2 13"', 'Laptop Apple MacBook Air con chip M2, 8GB RAM, 256GB SSD', 'LAP-APP-M2-007', @ComputadorasId, '/images/macbook-air-m2.jpg', 'Apple', '{"processor": "M2", "ram": "8GB", "storage": "256GB SSD", "screen": "13.6", "os": "macOS"}'),
('Laptop Dell XPS 15', 'Laptop premium Dell XPS 15 con Intel i7, 16GB RAM, 512GB SSD', 'LAP-DEL-XPS15-008', @ComputadorasId, '/images/dell-xps-15.jpg', 'Dell', '{"processor": "Intel i7", "ram": "16GB", "storage": "512GB SSD", "screen": "15.6", "graphics": "RTX 3050"}'),
('PC Gamer Custom RGB', 'PC de escritorio para gaming con RGB, AMD Ryzen 7, RTX 4070', 'PC-GAM-RGB-009', @ComputadorasId, '/images/pc-gamer-rgb.jpg', 'Custom', '{"processor": "AMD Ryzen 7", "ram": "32GB", "graphics": "RTX 4070", "storage": "1TB NVMe", "rgb": true}'),

-- Smartphones
('iPhone 15 Pro 128GB', 'iPhone 15 Pro con chip A17 Pro, cámara de 48MP, titanio', 'PHO-APP-15PRO-010', @SmartphonesId, '/images/iphone-15-pro.jpg', 'Apple', '{"storage": "128GB", "camera": "48MP", "processor": "A17 Pro", "material": "titanium", "5g": true}'),
('Samsung Galaxy S24 Ultra', 'Samsung Galaxy S24 Ultra con S Pen, cámara de 200MP', 'PHO-SAM-S24U-011', @SmartphonesId, '/images/galaxy-s24-ultra.jpg', 'Samsung', '{"storage": "256GB", "camera": "200MP", "processor": "Snapdragon 8 Gen 3", "spen": true, "5g": true}'),
('Google Pixel 8', 'Google Pixel 8 con Tensor G3, fotografía computacional avanzada', 'PHO-GOO-PIX8-012', @SmartphonesId, '/images/pixel-8.jpg', 'Google', '{"storage": "128GB", "camera": "50MP", "processor": "Tensor G3", "ai": true, "5g": true}'),

-- Tablets
('iPad Pro 12.9 M2', 'iPad Pro de 12.9 pulgadas con chip M2, pantalla Liquid Retina XDR', 'TAB-APP-PROM2-013', @TabletsId, '/images/ipad-pro-129-m2.jpg', 'Apple', '{"screen": "12.9", "processor": "M2", "storage": "128GB", "pencil": "Apple Pencil 2", "5g": true}'),
('Samsung Galaxy Tab S9', 'Tablet Samsung Galaxy Tab S9 con S Pen incluido', 'TAB-SAM-S9-014', @TabletsId, '/images/galaxy-tab-s9.jpg', 'Samsung', '{"screen": "11", "processor": "Snapdragon 8 Gen 2", "storage": "128GB", "spen": true, "waterproof": "IP68"}'),

-- Gaming
('PlayStation 5', 'Consola Sony PlayStation 5 con lector de discos', 'GAM-SON-PS5-015', @GamingId, '/images/playstation-5.jpg', 'Sony', '{"storage": "825GB SSD", "4k": true, "ray_tracing": true, "controller": "DualSense", "backwards": true}'),
('Xbox Series X', 'Consola Microsoft Xbox Series X, la más potente de Xbox', 'GAM-MIC-SERX-016', @GamingId, '/images/xbox-series-x.jpg', 'Microsoft', '{"storage": "1TB SSD", "4k": true, "ray_tracing": true, "fps": "120fps", "game_pass": true}'),
('Nintendo Switch OLED', 'Consola Nintendo Switch modelo OLED con pantalla de 7 pulgadas', 'GAM-NIN-SWOL-017', @GamingId, '/images/nintendo-switch-oled.jpg', 'Nintendo', '{"screen": "7 OLED", "storage": "64GB", "portable": true, "battery": "9h", "dock": true}'),

-- Fotografía
('Cámara Canon EOS R6', 'Cámara mirrorless full frame Canon EOS R6 Mark II', 'CAM-CAN-R6-018', @FotografiaId, '/images/canon-eos-r6.jpg', 'Canon', '{"sensor": "Full Frame", "megapixels": "24.2MP", "video": "4K", "stabilization": true, "lens_mount": "RF"}'),
('Lente Sony FE 24-70mm', 'Lente zoom estándar Sony FE 24-70mm f/2.8 GM II', 'LEN-SON-2470-019', @FotografiaId, '/images/sony-fe-24-70.jpg', 'Sony', '{"focal_length": "24-70mm", "aperture": "f/2.8", "mount": "Sony FE", "stabilization": true, "weather": true}'),

-- Cocina
('Cafetera Nespresso Vertuo', 'Cafetera Nespresso Vertuo Next con tecnología Centrifusion', 'COC-NES-VERT-020', @CocinaId, '/images/nespresso-vertuo.jpg', 'Nespresso', '{"capacity": "1.1L", "pressure": "centrifusion", "sizes": "multiple", "automatic": true, "bluetooth": true}'),
('Licuadora Vitamix A3500', 'Licuadora de alto rendimiento Vitamix Ascent A3500', 'COC-VIT-A3500-021', @CocinaId, '/images/vitamix-a3500.jpg', 'Vitamix', '{"power": "1400W", "capacity": "2L", "programs": "5", "wireless": true, "warranty": "10 years"}'),

-- Accesorios
('Apple Watch Series 9', 'Smartwatch Apple Watch Series 9 con GPS y cellular', 'ACC-APP-WS9-022', @AccesoríosId, '/images/apple-watch-s9.jpg', 'Apple', '{"screen": "45mm", "gps": true, "cellular": true, "health": true, "siri": true, "battery": "18h"}'),
('AirPods Pro 2da Gen', 'Auriculares Apple AirPods Pro de segunda generación con cancelación', 'ACC-APP-APPRO2-023', @AccesoríosId, '/images/airpods-pro-2.jpg', 'Apple', '{"noise_cancellation": true, "spatial_audio": true, "battery": "30h", "case": "MagSafe", "water_resistant": true}');

PRINT '✅ Productos insertados: 23 productos realistas';

-- ===============================================
-- 4. ASOCIACIONES PRODUCTO-PROVEEDOR CON PRECIOS
-- ===============================================

-- Obtener IDs de proveedores
DECLARE @TechSupplyId INT = (SELECT Id FROM Suppliers WHERE Name = 'TechSupply Corp');
DECLARE @ElectroMaxId INT = (SELECT Id FROM Suppliers WHERE Name = 'ElectroMax SA');
DECLARE @DigitalWorldId INT = (SELECT Id FROM Suppliers WHERE Name = 'Digital World');

-- Obtener algunos IDs de productos para las asociaciones
DECLARE @MonitorLGId INT = (SELECT Id FROM Products WHERE SKU = 'MON-LG27-4K-001');
DECLARE @MonitorSamsungId INT = (SELECT Id FROM Products WHERE SKU = 'MON-SAM32-GAM-002');
DECLARE @MacBookId INT = (SELECT Id FROM Products WHERE SKU = 'LAP-APP-M2-007');
DECLARE @iPhone15Id INT = (SELECT Id FROM Products WHERE SKU = 'PHO-APP-15PRO-010');
DECLARE @PS5Id INT = (SELECT Id FROM Products WHERE SKU = 'GAM-SON-PS5-015');
DECLARE @AppleWatchId INT = (SELECT Id FROM Products WHERE SKU = 'ACC-APP-WS9-022');

-- Insertar asociaciones con diferentes precios por proveedor
INSERT INTO ProductSuppliers (ProductId, SupplierId, Price, Stock, BatchNumber, SupplierSKU) VALUES 
-- Monitor LG - 3 proveedores diferentes precios
(@MonitorLGId, @TechSupplyId, 899.99, 15, 'BATCH-LG-2024-001', 'TS-LG27-4K'),
(@MonitorLGId, @ElectroMaxId, 949.99, 8, 'BATCH-LG-2024-002', 'EM-LG27-4K'),
(@MonitorLGId, @DigitalWorldId, 879.99, 22, 'BATCH-LG-2024-003', 'DW-LG27-4K'),

-- Monitor Samsung - 2 proveedores
(@MonitorSamsungId, @TechSupplyId, 1299.99, 12, 'BATCH-SAM-2024-001', 'TS-SAM32-GAM'),
(@MonitorSamsungId, @DigitalWorldId, 1249.99, 18, 'BATCH-SAM-2024-002', 'DW-SAM32-GAM'),

-- MacBook Air M2 - 3 proveedores (precios más altos)
(@MacBookId, @TechSupplyId, 1899.99, 5, 'BATCH-APP-2024-001', 'TS-MBA-M2'),
(@MacBookId, @ElectroMaxId, 1999.99, 3, 'BATCH-APP-2024-002', 'EM-MBA-M2'),
(@MacBookId, @DigitalWorldId, 1849.99, 7, 'BATCH-APP-2024-003', 'DW-MBA-M2'),

-- iPhone 15 Pro - 3 proveedores
(@iPhone15Id, @TechSupplyId, 1299.99, 25, 'BATCH-IPH-2024-001', 'TS-IPH15-PRO'),
(@iPhone15Id, @ElectroMaxId, 1349.99, 12, 'BATCH-IPH-2024-002', 'EM-IPH15-PRO'),
(@iPhone15Id, @DigitalWorldId, 1279.99, 30, 'BATCH-IPH-2024-003', 'DW-IPH15-PRO'),

-- PlayStation 5 - 2 proveedores (stock limitado)
(@PS5Id, @TechSupplyId, 699.99, 2, 'BATCH-PS5-2024-001', 'TS-PS5'),
(@PS5Id, @DigitalWorldId, 729.99, 1, 'BATCH-PS5-2024-002', 'DW-PS5'),

-- Apple Watch - 3 proveedores
(@AppleWatchId, @TechSupplyId, 549.99, 20, 'BATCH-AW-2024-001', 'TS-AW-S9'),
(@AppleWatchId, @ElectroMaxId, 579.99, 15, 'BATCH-AW-2024-002', 'EM-AW-S9'),
(@AppleWatchId, @DigitalWorldId, 539.99, 25, 'BATCH-AW-2024-003', 'DW-AW-S9');

-- Agregar más productos con menos proveedores para variedad
DECLARE @AirPodsId INT = (SELECT Id FROM Products WHERE SKU = 'ACC-APP-APPRO2-023');
DECLARE @iPadId INT = (SELECT Id FROM Products WHERE SKU = 'TAB-APP-PROM2-013');
DECLARE @SonyHeadphonesId INT = (SELECT Id FROM Products WHERE SKU = 'AUD-SON-WH1000-004');

INSERT INTO ProductSuppliers (ProductId, SupplierId, Price, Stock, BatchNumber, SupplierSKU) VALUES 
-- AirPods Pro - 2 proveedores
(@AirPodsId, @TechSupplyId, 299.99, 35, 'BATCH-APP-2024-004', 'TS-APP-PRO2'),
(@AirPodsId, @DigitalWorldId, 289.99, 42, 'BATCH-APP-2024-005', 'DW-APP-PRO2'),

-- iPad Pro - 1 proveedor (exclusivo)
(@iPadId, @ElectroMaxId, 1399.99, 6, 'BATCH-IPAD-2024-001', 'EM-IPAD-PRO'),

-- Sony Headphones - 3 proveedores
(@SonyHeadphonesId, @TechSupplyId, 449.99, 18, 'BATCH-SON-2024-001', 'TS-SON-WH1000'),
(@SonyHeadphonesId, @ElectroMaxId, 479.99, 10, 'BATCH-SON-2024-002', 'EM-SON-WH1000'),
(@SonyHeadphonesId, @DigitalWorldId, 439.99, 24, 'BATCH-SON-2024-003', 'DW-SON-WH1000');

PRINT '✅ Asociaciones Producto-Proveedor insertadas: 20 asociaciones';

-- ===============================================
-- 5. LISTAS DE DESEOS DE USUARIOS
-- ===============================================

-- Obtener IDs de usuarios
DECLARE @JuanId INT = (SELECT Id FROM Users WHERE Email = 'juan.perez@email.com');
DECLARE @MariaId INT = (SELECT Id FROM Users WHERE Email = 'maria.gonzalez@email.com');
DECLARE @CarlosId INT = (SELECT Id FROM Users WHERE Email = 'carlos.rodriguez@email.com');
DECLARE @AnaId INT = (SELECT Id FROM Users WHERE Email = 'ana.martinez@email.com');

-- Insertar items en wishlist
INSERT INTO Wishlists (UserId, ProductId, Notes) VALUES 
(@JuanId, @iPhone15Id, 'Para reemplazar mi iPhone 12'),
(@JuanId, @AppleWatchId, 'Combina perfecto con el iPhone'),
(@JuanId, @PS5Id, 'Para jugar FIFA 24'),

(@MariaId, @MacBookId, 'Para mi trabajo de diseño'),
(@MariaId, @iPadId, 'Para complementar el MacBook'),
(@MariaId, @AirPodsId, 'Los necesito para el gym'),

(@CarlosId, @MonitorSamsungId, 'Para mi setup gaming'),
(@CarlosId, @PS5Id, 'Esperando que baje de precio'),
(@CarlosId, @SonyHeadphonesId, 'Para trabajo remoto'),

(@AnaId, @iPhone15Id, 'Cuando esté en oferta'),
(@AnaId, @AppleWatchId, 'Para hacer ejercicio');

PRINT '✅ Lista de deseos insertada: 11 items';

-- ===============================================
-- 6. ÓRDENES DE EJEMPLO
-- ===============================================

-- Crear algunas órdenes usando el stored procedure
DECLARE @OrderItemsJson1 NVARCHAR(MAX) = '[
    {"ProductSupplierId": ' + CAST((SELECT Id FROM ProductSuppliers WHERE ProductId = @AppleWatchId AND SupplierId = @DigitalWorldId) AS VARCHAR) + ', "Quantity": 1},
    {"ProductSupplierId": ' + CAST((SELECT Id FROM ProductSuppliers WHERE ProductId = @AirPodsId AND SupplierId = @DigitalWorldId) AS VARCHAR) + ', "Quantity": 1}
]';

EXEC sp_CreateOrder 
    @UserId = @JuanId,
    @ShippingAddress = 'Calle 50, Edificio Torre Global, Piso 15, Ciudad de Panamá, Panamá',
    @OrderItems = @OrderItemsJson1,
    @Notes = 'Entrega en horario de oficina';

-- Segunda orden
DECLARE @OrderItemsJson2 NVARCHAR(MAX) = '[
    {"ProductSupplierId": ' + CAST((SELECT Id FROM ProductSuppliers WHERE ProductId = @SonyHeadphonesId AND SupplierId = @DigitalWorldId) AS VARCHAR) + ', "Quantity": 1}
]';

EXEC sp_CreateOrder 
    @UserId = @CarlosId,
    @ShippingAddress = 'Avenida Balboa, Residencial Ocean View, Torre A, Apt 25B, Ciudad de Panamá',
    @OrderItems = @OrderItemsJson2,
    @Notes = 'Portero recibe paquetes';

-- Tercera orden (más grande)
DECLARE @OrderItemsJson3 NVARCHAR(MAX) = '[
    {"ProductSupplierId": ' + CAST((SELECT Id FROM ProductSuppliers WHERE ProductId = @MonitorLGId AND SupplierId = @DigitalWorldId) AS VARCHAR) + ', "Quantity": 2},
    {"ProductSupplierId": ' + CAST((SELECT Id FROM ProductSuppliers WHERE ProductId = @iPhone15Id AND SupplierId = @DigitalWorldId) AS VARCHAR) + ', "Quantity": 1}
]';

EXEC sp_CreateOrder 
    @UserId = @MariaId,
    @ShippingAddress = 'Costa del Este, Boulevard Costa del Este, Casa 123, Ciudad de Panamá',
    @OrderItems = @OrderItemsJson3,
    @Notes = 'Oficina en casa - disponible todo el día';

PRINT '✅ Órdenes de ejemplo creadas: 3 órdenes';

-- ===============================================
-- 7. ACTUALIZAR ALGUNAS ÓRDENES PARA VARIEDAD
-- ===============================================

-- Confirmar la primera orden
UPDATE Orders 
SET Status = 'Confirmed', 
    UpdatedAt = GETDATE() 
WHERE OrderNumber LIKE 'ORD-%' 
  AND Id = (SELECT MIN(Id) FROM Orders WHERE UserId = @JuanId);

-- Marcar la segunda como enviada
UPDATE Orders 
SET Status = 'Shipped', 
    UpdatedAt = GETDATE(),
    DeliveryDate = DATEADD(DAY, 2, GETDATE())
WHERE OrderNumber LIKE 'ORD-%' 
  AND Id = (SELECT MIN(Id) FROM Orders WHERE UserId = @CarlosId);

PRINT '✅ Estados de órdenes actualizados';

-- ===============================================
-- 8. GENERAR DATOS HISTÓRICOS SIMULADOS
-- ===============================================

-- Simular algunas ventas del mes pasado (para reportes)
INSERT INTO Orders (OrderNumber, UserId, TotalAmount, Status, OrderDate, ShippingAddress, CreatedAt, UpdatedAt) VALUES 
('ORD-20241201-0001', @JuanId, 1589.98, 'Delivered', '2024-12-01 10:30:00', 'Dirección histórica 1', '2024-12-01 10:30:00', '2024-12-05 16:20:00'),
('ORD-20241205-0001', @MariaId, 449.99, 'Delivered', '2024-12-05 14:15:00', 'Dirección histórica 2', '2024-12-05 14:15:00', '2024-12-08 11:10:00'),
('ORD-20241210-0001', @AnaId, 729.99, 'Delivered', '2024-12-10 09:45:00', 'Dirección histórica 3', '2024-12-10 09:45:00', '2024-12-13 15:30:00');

-- Insertar detalles para las órdenes históricas
DECLARE @HistOrder1 INT = (SELECT Id FROM Orders WHERE OrderNumber = 'ORD-20241201-0001');
DECLARE @HistOrder2 INT = (SELECT Id FROM Orders WHERE OrderNumber = 'ORD-20241205-0001');
DECLARE @HistOrder3 INT = (SELECT Id FROM Orders WHERE OrderNumber = 'ORD-20241210-0001');

INSERT INTO OrderDetails (OrderId, ProductSupplierId, Quantity, UnitPrice, CreatedAt) VALUES 
(@HistOrder1, (SELECT Id FROM ProductSuppliers WHERE ProductId = @iPhone15Id AND SupplierId = @DigitalWorldId), 1, 1279.99, '2024-12-01 10:30:00'),
(@HistOrder1, (SELECT Id FROM ProductSuppliers WHERE ProductId = @AirPodsId AND SupplierId = @DigitalWorldId), 1, 289.99, '2024-12-01 10:30:00'),

(@HistOrder2, (SELECT Id FROM ProductSuppliers WHERE ProductId = @SonyHeadphonesId AND SupplierId = @DigitalWorldId), 1, 439.99, '2024-12-05 14:15:00'),

(@HistOrder3, (SELECT Id FROM ProductSuppliers WHERE ProductId = @PS5Id AND SupplierId = @DigitalWorldId), 1, 729.99, '2024-12-10 09:45:00');

PRINT '✅ Datos históricos simulados insertados';

-- ===============================================
-- 9. RESUMEN FINAL DE DATOS
-- ===============================================

PRINT '';
PRINT '===============================================';
PRINT '           DATOS DE PRUEBA COMPLETADOS        ';
PRINT '===============================================';
PRINT '';

--Re declaración de la variable
DECLARE @CustomerRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Customer');

-- Mostrar estadísticas de los datos insertados
SELECT 'Usuarios' AS Tabla, COUNT(*) AS Cantidad FROM Users WHERE RoleId = @CustomerRoleId
UNION ALL
SELECT 'Productos', COUNT(*) FROM Products WHERE IsActive = 1
UNION ALL
SELECT 'Categorías', COUNT(*) FROM Categories WHERE IsActive = 1
UNION ALL
SELECT 'Proveedores', COUNT(*) FROM Suppliers WHERE IsActive = 1
UNION ALL
SELECT 'Asociaciones P-P', COUNT(*) FROM ProductSuppliers WHERE IsActive = 1
UNION ALL
SELECT 'Items Wishlist', COUNT(*) FROM Wishlists
UNION ALL
SELECT 'Órdenes', COUNT(*) FROM Orders
UNION ALL
SELECT 'Detalles Orden', COUNT(*) FROM OrderDetails;

PRINT '';
PRINT '📊 ESTADÍSTICAS GENERADAS:';

-- Mostrar algunos datos de ejemplo usando las views
SELECT 'PRODUCTOS MÁS BARATOS:' AS Consulta;
SELECT TOP 5 
    ProductName, 
    dbo.fn_FormatPrice(MinPrice, 'USD') AS PrecioMinimo,
    TotalStock,
    SupplierCount AS Proveedores
FROM vw_ProductsCatalog 
WHERE IsAvailable = 1
ORDER BY MinPrice ASC;

PRINT '';
SELECT 'INVENTARIO POR PROVEEDOR:' AS Consulta;
SELECT 
    SupplierName,
    ProductCount AS Productos,
    dbo.fn_FormatPrice(TotalInventoryValue, 'USD') AS ValorInventario
FROM vw_SupplierStats
ORDER BY TotalInventoryValue DESC;

PRINT '';
PRINT '✅ DATOS DE PRUEBA LISTOS PARA:';
PRINT '   - Testing de API endpoints';
PRINT '   - Pruebas de funcionalidad móvil';
PRINT '   - Validación de reportes';
PRINT '   - Demo del sistema completo';
PRINT '';
PRINT '🚀 SIGUIENTE PASO: Scripts de Testing/Validación';
PRINT '===============================================';

GO