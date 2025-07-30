using AutoMapper;
using InvenBank.API.DTOs;
using InvenBank.API.DTOs.Requests;
using InvenBank.API.Entities;

namespace InvenBank.API.Configuration
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // CreateProductRequest -> Product
            CreateMap<CreateProductRequest, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlists, opt => opt.Ignore());

            // UpdateProductRequest -> Product
            CreateMap<UpdateProductRequest, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlists, opt => opt.Ignore());

            // Product -> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryPath, opt => opt.Ignore())
                .ForMember(dest => dest.MinPrice, opt => opt.Ignore())
                .ForMember(dest => dest.MaxPrice, opt => opt.Ignore())
                .ForMember(dest => dest.BestPrice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierCount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalSold, opt => opt.Ignore())
                .ForMember(dest => dest.TotalRevenue, opt => opt.Ignore())
                .ForMember(dest => dest.WishlistCount, opt => opt.Ignore())
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.LastRestockDate, opt => opt.Ignore());

            // Product -> ProductDetailDto
            CreateMap<Product, ProductDetailDto>()
                .IncludeBase<Product, ProductDto>()
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.ParsedSpecifications, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedProducts, opt => opt.Ignore());

            // Product -> ProductCatalogDto
            CreateMap<Product, ProductCatalogDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.MinPrice, opt => opt.Ignore())
                .ForMember(dest => dest.MaxPrice, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierCount, opt => opt.Ignore());

            // Product -> RelatedProductDto
            CreateMap<Product, RelatedProductDto>()
                .ForMember(dest => dest.MinPrice, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore());

            // ProductSupplier -> ProductSupplierDto
            CreateMap<ProductSupplier, ProductSupplierDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSKU, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierName, opt => opt.Ignore());

            // ProductImportDto -> Product
            CreateMap<ProductImportDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Wishlists, opt => opt.Ignore());

            // Product -> ProductExportDto
            CreateMap<Product, ProductExportDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.MinPrice, opt => opt.Ignore())
                .ForMember(dest => dest.MaxPrice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalStock, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierCount, opt => opt.Ignore())
                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => "Disponible"));
        }
    }
}