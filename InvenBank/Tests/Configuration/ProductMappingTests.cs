//using AutoMapper;
//using InvenBank.API.Configuration;
//using InvenBank.API.DTOs;
//using InvenBank.API.DTOs.Requests;
//using InvenBank.API.Entities;
//using Microsoft.Extensions.Logging;
//using Xunit;

//namespace InvenBank.API.Tests.Configuration
//{
//    /// <summary>
//    /// Pruebas unitarias para validar que los mapeos de productos funcionan correctamente
//    /// </summary>
//    public class ProductMappingTests
//    {
//        private readonly IMapper _mapper;

//        public ProductMappingTests()
//        {
//            // Configurar AutoMapper con todos los perfiles
//            var configuration = new MapperConfiguration(cfg =>
//            {
//                cfg.AddProfile<AutoMapperProfile>();
//                cfg.AddProfile<ProductMappingProfile>();
//            });

//            configuration.AssertConfigurationIsValid(); // Validar configuración
//            _mapper = configuration.CreateMapper();
//        }

//        // ===============================================
//        // PRUEBAS DE MAPEO BÁSICO DE PRODUCTOS
//        // ===============================================

//        [Fact]
//        public void Should_Map_CreateProductRequest_To_Product()
//        {
//            // Arrange
//            var request = new CreateProductRequest
//            {
//                Name = "Laptop Gaming Asus ROG",
//                Description = "Laptop gaming de alta gama con RTX 4080",
//                SKU = "LAP-ASUS-ROG-001",
//                CategoryId = 1,
//                ImageUrl = "https://example.com/laptop.jpg",
//                Brand = "ASUS",
//                Specifications = "{\"processor\": \"Intel i9\", \"ram\": \"32GB\", \"graphics\": \"RTX 4080\"}"
//            };

//            // Act
//            var product = _mapper.Map<Product>(request);

//            // Assert
//            Assert.Equal(request.Name, product.Name);
//            Assert.Equal(request.Description, product.Description);
//            Assert.Equal(request.SKU, product.SKU);
//            Assert.Equal(request.CategoryId, product.CategoryId);
//            Assert.Equal(request.ImageUrl, product.ImageUrl);
//            Assert.Equal(request.Brand, product.Brand);
//            Assert.True(product.IsActive);
//            Assert.NotNull(product.Specifications);
//        }

//        [Fact]
//        public void Should_Map_Product_To_ProductDto()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Name = "iPhone 15 Pro",
//                Description = "Smartphone Apple con chip A17 Pro",
//                SKU = "IPH-15-PRO-001",
//                CategoryId = 2,
//                ImageUrl = "https://example.com/iphone.jpg",
//                Brand = "Apple",
//                Specifications = "{\"storage\": \"256GB\", \"camera\": \"48MP\", \"5g\": true}",
//                IsActive = true,
//                CreatedAt = DateTime.Now,
//                UpdatedAt = DateTime.Now
//            };

//            // Act
//            var dto = _mapper.Map<ProductDto>(product);

//            // Assert
//            Assert.Equal(product.Id, dto.Id);
//            Assert.Equal(product.Name, dto.Name);
//            Assert.Equal(product.Description, dto.Description);
//            Assert.Equal(product.SKU, dto.SKU);
//            Assert.Equal(product.CategoryId, dto.CategoryId);
//            Assert.Equal(product.ImageUrl, dto.ImageUrl);
//            Assert.Equal(product.Brand, dto.Brand);
//            Assert.Equal(product.IsActive, dto.IsActive);
//        }

//        [Fact]
//        public void Should_Map_UpdateProductRequest_To_Product()
//        {
//            // Arrange
//            var request = new UpdateProductRequest
//            {
//                Name = "MacBook Pro M3 Updated",
//                Description = "MacBook Pro actualizado con chip M3",
//                SKU = "MBP-M3-UPD-001",
//                CategoryId = 1,
//                ImageUrl = "https://example.com/macbook-updated.jpg",
//                Brand = "Apple",
//                Specifications = "{\"processor\": \"M3 Pro\", \"ram\": \"16GB\", \"storage\": \"512GB SSD\"}",
//                IsActive = true
//            };

//            // Act
//            var product = _mapper.Map<Product>(request);

//            // Assert
//            Assert.Equal(request.Name, product.Name);
//            Assert.Equal(request.IsActive, product.IsActive);
//            Assert.NotNull(product.Specifications);
//        }

//        // ===============================================
//        // PRUEBAS DE MAPEO DE DETALLES
//        // ===============================================

//        [Fact]
//        public void Should_Map_Product_To_ProductDetailDto()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Name = "Gaming Monitor 4K",
//                Description = "Monitor gaming 4K 144Hz",
//                SKU = "MON-GAM-4K-001",
//                CategoryId = 3,
//                Brand = "Samsung",
//                Specifications = "{\"resolution\": \"3840x2160\", \"refresh_rate\": \"144Hz\", \"hdr\": true, \"size\": 27}",
//                IsActive = true
//            };

//            // Act
//            var detailDto = _mapper.Map<ProductDetailDto>(product);

//            // Assert
//            Assert.Equal(product.Id, detailDto.Id);
//            Assert.Equal(product.Name, detailDto.Name);
//            Assert.NotNull(detailDto.ParsedSpecifications);
//            Assert.True(detailDto.ParsedSpecifications.Count > 0);

//            // Verificar que las especificaciones se parsearon correctamente
//            var resolutionSpec = detailDto.ParsedSpecifications.FirstOrDefault(s => s.Key.Contains("Resolution"));
//            Assert.NotNull(resolutionSpec);
//            Assert.Equal("3840x2160", resolutionSpec.Value);
//        }

//        // ===============================================
//        // PRUEBAS DE MAPEO DE CATÁLOGO
//        // ===============================================

//        [Fact]
//        public void Should_Map_Product_To_ProductCatalogDto()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Name = "Wireless Headphones",
//                Description = "Premium wireless headphones with ANC",
//                SKU = "HDI-WIR-ANC-001",
//                CategoryId = 4,
//                ImageUrl = "https://example.com/headphones.jpg",
//                Brand = "Sony",
//                IsActive = true
//            };

//            // Act
//            var catalogDto = _mapper.Map<ProductCatalogDto>(product);

//            // Assert
//            Assert.Equal(product.Id, catalogDto.Id);
//            Assert.Equal(product.Name, catalogDto.Name);
//            Assert.Equal(product.Description, catalogDto.Description);
//            Assert.Equal(product.SKU, catalogDto.SKU);
//            Assert.Equal(product.ImageUrl, catalogDto.ImageUrl);
//            Assert.Equal(product.Brand, catalogDto.Brand);
//        }

//        // ===============================================
//        // PRUEBAS DE MAPEO DE IMPORT/EXPORT
//        // ===============================================

//        [Fact]
//        public void Should_Map_ProductImportDto_To_Product()
//        {
//            // Arrange
//            var importDto = new ProductImportDto
//            {
//                Name = "Imported Tablet",
//                Description = "Tablet importada desde CSV",
//                SKU = "", // SKU vacío para probar auto-generación
//                CategoryName = "Tablets",
//                Brand = "Generic",
//                ImageUrl = "https://example.com/tablet.jpg",
//                Specifications = "{\"screen\": \"10.1\", \"storage\": \"64GB\"}"
//            };

//            // Act
//            var product = _mapper.Map<Product>(importDto);

//            // Assert
//            Assert.Equal(importDto.Name, product.Name);
//            Assert.Equal(importDto.Description, product.Description);
//            Assert.Equal(importDto.Brand, product.Brand);
//            Assert.True(product.IsActive);
//            Assert.NotEmpty(product.SKU); // SKU debería haberse generado automáticamente
//        }

//        [Fact]
//        public void Should_Map_Product_To_ProductExportDto()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Name = "Export Test Product",
//                SKU = "ETP-001",
//                Brand = "TestBrand",
//                IsActive = true,
//                CreatedAt = DateTime.Now
//            };

//            // Act
//            var exportDto = _mapper.Map<ProductExportDto>(product);

//            // Assert
//            Assert.Equal(product.Name, exportDto.Name);
//            Assert.Equal(product.SKU, exportDto.SKU);
//            Assert.Equal(product.Brand, exportDto.Brand);
//            Assert.Equal(product.IsActive, exportDto.IsActive);
//            Assert.Equal(product.CreatedAt, exportDto.CreatedAt);
//        }

//        // ===============================================
//        // PRUEBAS DE MAPEO DE PROVEEDORES
//        // ===============================================

//        [Fact]
//        public void Should_Map_ProductSupplier_To_ProductSupplierDto()
//        {
//            // Arrange
//            var productSupplier = new ProductSupplier
//            {
//                Id = 1,
//                ProductId = 1,
//                SupplierId = 1,
//                Price = 299.99m,
//                Stock = 50,
//                BatchNumber = "BATCH-2024-001",
//                SupplierSKU = "SUP-SKU-001",
//                LastRestockDate = DateTime.Now.AddDays(-5),
//                IsActive = true
//            };

//            // Act
//            var dto = _mapper.Map<ProductSupplierDto>(productSupplier);

//            // Assert
//            Assert.Equal(productSupplier.Id, dto.Id);
//            Assert.Equal(productSupplier.ProductId, dto.ProductId);
//            Assert.Equal(productSupplier.SupplierId, dto.SupplierId);
//            Assert.Equal(productSupplier.Price, dto.Price);
//            Assert.Equal(productSupplier.Stock, dto.Stock);
//            Assert.Equal(productSupplier.BatchNumber, dto.BatchNumber);
//            Assert.Equal(productSupplier.SupplierSKU, dto.SupplierSKU);
//            Assert.Equal(productSupplier.LastRestockDate, dto.LastRestockDate);
//        }

//        // ===============================================
//        // PRUEBAS DE VALIDACIÓN DE CONFIGURACIÓN
//        // ===============================================

//        [Fact]
//        public void AutoMapper_Configuration_Should_Be_Valid()
//        {
//            // Arrange & Act & Assert
//            var configuration = new MapperConfiguration(cfg =>
//            {
//                cfg.AddProfile<AutoMapperProfile>();
//                cfg.AddProfile<ProductMappingProfile>();
//            });

//            // Esto lanzará una excepción si la configuración no es válida
//            Assert.NotNull(configuration);
//            configuration.AssertConfigurationIsValid();
//        }

//        // ===============================================
//        // PRUEBAS DE CASOS EDGE
//        // ===============================================

//        [Fact]
//        public void Should_Handle_Null_Specifications_Gracefully()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Name = "Product Without Specs",
//                SKU = "PWS-001",
//                CategoryId = 1,
//                Specifications = null, // Especificaciones nulas
//                IsActive = true
//            };

//            // Act
//            var detailDto = _mapper.Map<ProductDetailDto>(product);

//            // Assert
//            Assert.NotNull(detailDto.ParsedSpecifications);
//            Assert.Empty(detailDto.ParsedSpecifications);
//        }

//        [Fact]
//        public void Should_Handle_Invalid_Json_Specifications()
//        {
//            // Arrange
//            var product = new Product
//            {
//                Id = 1,
//                Name = "Product With Invalid JSON",
//                SKU = "PWIJ-001",
//                CategoryId = 1,
//                Specifications = "{invalid json", // JSON inválido
//                IsActive = true
//            };

//            // Act
//            var detailDto = _mapper.Map<ProductDetailDto>(product);

//            // Assert
//            Assert.NotNull(detailDto.ParsedSpecifications);
//            Assert.Empty(detailDto.ParsedSpecifications); // Debería manejar graciosamente el JSON inválido
//        }

//        [Fact]
//        public void Should_Generate_SKU_When_Empty()
//        {
//            // Arrange
//            var importDto = new ProductImportDto
//            {
//                Name = "Auto SKU Product",
//                SKU = "", // SKU vacío
//                CategoryName = "Test Category"
//            };

//            // Act  
//            var product = _mapper.Map<Product>(importDto);

//            // Assert
//            Assert.NotEmpty(product.SKU);
//            Assert.Contains("AUT", product.SKU); // Debería contener parte del nombre
//        }
//    }

//    /// <summary>
//    /// Pruebas de integración para validar los helpers de mapeo
//    /// </summary>
//    public class ProductMappingHelpersTests
//    {
//        [Theory]
//        [InlineData("LAP-ASUS-ROG-001", true)]
//        [InlineData("SIMPLE-SKU", true)]
//        [InlineData("123-ABC-456", true)]
//        [InlineData("", false)]
//        [InlineData("AB", false)] // Muy corto
//        [InlineData("-STARTS-WITH-DASH", false)]
//        [InlineData("ENDS-WITH-DASH-", false)]
//        [InlineData("HAS SPACES", false)]
//        [InlineData("has@special!chars", false)]
//        public void Should_Validate_SKU_Format_Correctly(string sku, bool expectedValid)
//        {
//            // Act
//            var isValid = ProductMappingHelpers.IsValidSKU(sku);

//            // Assert
//            Assert.Equal(expectedValid, isValid);
//        }

//        [Theory]
//        [InlineData("{\"key\": \"value\"}", true)]
//        [InlineData("{\"number\": 123, \"boolean\": true}", true)]
//        [InlineData("", true)] // Empty es válido
//        [InlineData(null, true)] // Null es válido
//        [InlineData("{invalid json", false)]
//        [InlineData("not json at all", false)]
//        public void Should_Validate_JSON_Specifications_Correctly(string? json, bool expectedValid)
//        {
//            // Act
//            var isValid = ProductMappingHelpers.IsValidSpecificationsJson(json);

//            // Assert
//            Assert.Equal(expectedValid, isValid);
//        }

//        [Theory]
//        [InlineData(0, "Sin Stock")]
//        [InlineData(3, "Stock Bajo")]
//        [InlineData(10, "Stock Normal")]
//        [InlineData(25, "Stock Alto")]
//        [InlineData(100, "Stock Abundante")]
//        public void Should_Determine_Stock_Status_Correctly(int stock, string expectedStatus)
//        {
//            // Act
//            var status = ProductMappingHelpers.DetermineStockStatus(stock);

//            // Assert
//            Assert.Equal(expectedStatus, status);
//        }

//        [Fact]
//        public void Should_Generate_Auto_SKU_With_Proper_Format()
//        {
//            // Arrange
//            var productName = "Gaming Laptop Supreme";
//            var categoryName = "Computers";

//            // Act
//            var sku = ProductMappingHelpers.GenerateAutoSKU(productName, categoryName);

//            // Assert
//            Assert.NotEmpty(sku);
//            Assert.Contains("CO", sku); // Parte de "Computers"
//            Assert.Contains("GAM", sku); // Parte de "Gaming"
//            Assert.True(ProductMappingHelpers.IsValidSKU(sku));
//        }

//        [Fact]
//        public void Should_Parse_Specifications_Json_To_Items()
//        {
//            // Arrange
//            var json = "{\"processor\": \"Intel i7\", \"ram\": \"16GB\", \"ssd\": true, \"cores\": 8}";

//            // Act
//            var items = ProductMappingHelpers.ParseSpecifications(json);

//            // Assert
//            Assert.Equal(4, items.Count);

//            var processorItem = items.FirstOrDefault(i => i.Key.Contains("Processor"));
//            Assert.NotNull(processorItem);
//            Assert.Equal("Intel i7", processorItem.Value);
//            Assert.Equal("text", processorItem.Type);

//            var coresItem = items.FirstOrDefault(i => i.Key.Contains("Cores"));
//            Assert.NotNull(coresItem);
//            Assert.Equal("8", coresItem.Value);
//            Assert.Equal("number", coresItem.Type);
//        }
//    }
//}