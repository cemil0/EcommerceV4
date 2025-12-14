using AutoMapper;
using ECommerce.Application.Configuration;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ECommerce.Tests.Integration;

public class ProductServiceIntegrationTests : IDisposable
{
    private readonly ECommerceDbContext _context;
    private readonly ProductService _productService;
    private readonly IMapper _mapper;

    public ProductServiceIntegrationTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ECommerceDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<ProductVariant, ProductVariantDto>();
            cfg.CreateMap<Category, CategoryDto>();
        });
        _mapper = config.CreateMapper();

        // Setup repositories and service
        var productRepository = new ProductRepository(_context);
        var orderItemRepository = new OrderItemRepository(_context);
        var orderStatusHistoryRepository = new OrderStatusHistoryRepository(_context);
        var unitOfWork = new UnitOfWork(_context, productRepository, null!, null!, null!, null!, null!, null!, null!, null!, null!, orderItemRepository, orderStatusHistoryRepository);
        
        // Mock required services
        var cacheMock = new Mock<ICacheService>();
        var loggerMock = new Mock<ILogger<ProductService>>();
        var cacheOptions = Options.Create(new CacheOptions());
        
        _productService = new ProductService(unitOfWork, _mapper, cacheMock.Object, loggerMock.Object, cacheOptions);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var category = new Category
        {
            CategoryId = 1,
            CategoryName = "Electronics",
            CategorySlug = "electronics",
            IsActive = true
        };

        var products = new List<Product>
        {
            new Product
            {
                ProductId = 1,
                SKU = "LAPTOP-001",
                ProductName = "Dell XPS 15",
                ProductSlug = "dell-xps-15",
                ShortDescription = "High performance laptop",
                Brand = "Dell",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 1,
                Category = category,
                ProductVariants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        ProductVariantId = 1,
                        VariantSKU = "LAPTOP-001-16GB",
                        VariantName = "16GB RAM",
                        BasePrice = 45000,
                        SalePrice = 42000,
                        Currency = "TRY",
                        IsActive = true
                    }
                }
            },
            new Product
            {
                ProductId = 2,
                SKU = "PHONE-001",
                ProductName = "iPhone 15",
                ProductSlug = "iphone-15",
                ShortDescription = "Latest iPhone",
                Brand = "Apple",
                IsActive = true,
                IsFeatured = false,
                CategoryId = 1,
                Category = category
            },
            new Product
            {
                ProductId = 3,
                SKU = "TABLET-001",
                ProductName = "iPad Pro",
                ProductSlug = "ipad-pro",
                ShortDescription = "Professional tablet",
                Brand = "Apple",
                IsActive = false, // Inactive product
                IsFeatured = false,
                CategoryId = 1,
                Category = category
            }
        };

        _context.Categories.Add(category);
        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProductWithRelations()
    {
        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ProductId.Should().Be(1);
        result.ProductName.Should().Be("Dell XPS 15");
        result.SKU.Should().Be("LAPTOP-001");
        result.Brand.Should().Be("Dell");
        result.IsActive.Should().BeTrue();
        result.IsFeatured.Should().BeTrue();
        
        // Check relations
        result.Category.Should().NotBeNull();
        result.Category!.CategoryName.Should().Be("Electronics");
        result.ProductVariants.Should().HaveCount(1);
        result.ProductVariants.First().VariantName.Should().Be("16GB RAM");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _productService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveProducts()
    {
        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only active products
        result.Should().OnlyContain(p => p.IsActive);
        result.Should().Contain(p => p.ProductName == "Dell XPS 15");
        result.Should().Contain(p => p.ProductName == "iPhone 15");
        result.Should().NotContain(p => p.ProductName == "iPad Pro"); // Inactive
    }

    [Fact]
    public async Task GetBySlugAsync_WithValidSlug_ReturnsProduct()
    {
        // Act
        var result = await _productService.GetBySlugAsync("dell-xps-15");

        // Assert
        result.Should().NotBeNull();
        result!.ProductSlug.Should().Be("dell-xps-15");
        result.ProductName.Should().Be("Dell XPS 15");
    }

    [Fact]
    public async Task GetBySlugAsync_WithInvalidSlug_ReturnsNull()
    {
        // Act
        var result = await _productService.GetBySlugAsync("non-existent-product");

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
