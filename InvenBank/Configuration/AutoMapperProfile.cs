using AutoMapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.Entities;

namespace InvenBank.API.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            ConfigureCategoryMappings();
            ConfigureUserMappings();
            ConfigureSupplierMappings();
            ConfigureProductMappings();
        }

        // ===============================================
        // MAPEOS DE CATEGORÍAS
        // ===============================================

        private void ConfigureCategoryMappings()
        {
            // Category Entity <-> CategoryDto
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentName, opt => opt.Ignore()) // Se llena desde la consulta
                .ForMember(dest => dest.ProductCount, opt => opt.Ignore()) // Se llena desde la consulta
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ReverseMap();

            // CreateCategoryRequest -> Category Entity
            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // UpdateCategoryRequest -> Category Entity
            CreateMap<UpdateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        }

        // ===============================================
        // MAPEOS DE USUARIOS
        // ===============================================

        private void ConfigureUserMappings()
        {
            // User Entity <-> UserDto
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
                .ReverseMap()
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlists, opt => opt.Ignore());

            // CreateUserRequest -> User Entity
            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Se maneja en el servicio
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlists, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore());
        }

        // ===============================================
        // MAPEOS DE PROVEEDORES
        // ===============================================

        private void ConfigureSupplierMappings()
        {
            // Supplier Entity <-> SupplierDto
            CreateMap<Supplier, SupplierDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.Ignore()) // Se llena desde la consulta
                .ForMember(dest => dest.TotalInventoryValue, opt => opt.Ignore()) // Se llena desde la consulta
                .ReverseMap()
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore());

            // CreateSupplierRequest -> Supplier Entity
            CreateMap<CreateSupplierRequest, Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore());

            // UpdateSupplierRequest -> Supplier Entity
            CreateMap<UpdateSupplierRequest, Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore());
        }

        // ===============================================
        // MAPEOS DE PRODUCTOS
        // ===============================================

        private void ConfigureProductMappings()
        {
            // Product Entity <-> ProductDto (se definirá cuando creemos ProductDto)
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.MinPrice, opt => opt.Ignore()) // Se llena desde la consulta
                .ForMember(dest => dest.MaxPrice, opt => opt.Ignore()) // Se llena desde la consulta
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore()) // Se llena desde la consulta
                .ForMember(dest => dest.SupplierCount, opt => opt.Ignore()) // Se llena desde la consulta
                .ReverseMap()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlists, opt => opt.Ignore());

            // ProductSupplier Entity <-> ProductSupplierDto
            CreateMap<ProductSupplier, ProductSupplierDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
                .ReverseMap()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());

            // Order Entity <-> OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderDetails.Count))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());

            // Wishlist Entity <-> WishlistDto
            CreateMap<Wishlist, WishlistDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.Product.Brand))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }
    }
}
