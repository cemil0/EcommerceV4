using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => 0)); // Will be populated from inventory

        // Category mappings
        CreateMap<Category, CategoryDto>();

        // Cart mappings
        CreateMap<Cart, CartDto>();
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.ProductName))
            .ForMember(dest => dest.VariantName, opt => opt.MapFrom(src => src.ProductVariant.VariantName));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType.ToString()))
            .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.OrderStatus.ToString()));
        CreateMap<OrderItem, OrderItemDto>();

        // Customer mappings
        CreateMap<Customer, CustomerDto>();
        CreateMap<UpdateCustomerRequest, Customer>();

        // Address mappings
        CreateMap<Address, AddressDto>();
        CreateMap<CreateAddressRequest, Address>();
        CreateMap<UpdateAddressRequest, Address>();

        // Company mappings
        CreateMap<Company, CompanyDto>();
        CreateMap<UpdateCompanyRequest, Company>();
    }
}
