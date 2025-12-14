using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerce.Tests.Integration;

public class OrderServiceIntegrationTests : IDisposable
{
    private readonly ECommerceDbContext _context;
    private readonly OrderService _orderService;
    private readonly IMapper _mapper;

    public OrderServiceIntegrationTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ECommerceDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType.ToString()))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.OrderStatus.ToString()));
            cfg.CreateMap<OrderItem, OrderItemDto>();
        });
        _mapper = config.CreateMapper();

        // Setup repositories and service
        var orderRepository = new OrderRepository(_context);
        var orderItemRepository = new OrderItemRepository(_context);
        var orderStatusHistoryRepository = new OrderStatusHistoryRepository(_context);
        var unitOfWork = new UnitOfWork(_context, null!, null!, null!, null!, null!, null!, null!, null!, null!, orderRepository, orderItemRepository, orderStatusHistoryRepository);
        
        // Mock required services
        var stockReservationService = new Mock<IStockReservationService>().Object;
        var priceValidationService = new Mock<IPriceValidationService>().Object;
        var orderBusinessRules = new Mock<IOrderBusinessRules>().Object;
        var orderStateMachine = new Mock<IOrderStateMachine>().Object;
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<OrderService>>().Object;
        
        _orderService = new OrderService(
            unitOfWork, 
            _mapper, 
            stockReservationService, 
            priceValidationService, 
            orderBusinessRules, 
            orderStateMachine, 
            logger
        );

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var orders = new List<Order>
        {
            new Order
            {
                OrderId = 1,
                OrderNumber = "ORD-2025-000001",
                OrderType = OrderType.B2C,
                OrderStatus = OrderStatus.Pending,
                SubtotalAmount = 1000m,
                TaxAmount = 200m,
                ShippingAmount = 50m,
                TotalAmount = 1250m,
                Currency = "TRY",
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Order
            {
                OrderId = 2,
                OrderNumber = "ORD-2025-000002",
                OrderType = OrderType.B2B,
                OrderStatus = OrderStatus.Approved,
                SubtotalAmount = 5000m,
                TaxAmount = 1000m,
                ShippingAmount = 0m,
                TotalAmount = 6000m,
                Currency = "TRY",
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Orders.AddRange(orders);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_GeneratesCorrectFormat()
    {
        // Act
        var result = await _orderService.GenerateOrderNumberAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("ORD-");
        result.Should().MatchRegex(@"^ORD-\d{4}-\d{6}$");
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_IncrementsSequentially()
    {
        // Act
        var firstNumber = await _orderService.GenerateOrderNumberAsync();
        
        // Add an order to increment count
        var newOrder = new Order
        {
            OrderNumber = firstNumber,
            OrderType = OrderType.B2C,
            OrderStatus = OrderStatus.Pending,
            TotalAmount = 100m,
            Currency = "TRY",
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        var secondNumber = await _orderService.GenerateOrderNumberAsync();

        // Assert
        firstNumber.Should().EndWith("000003"); // 2 existing + 1 = 3
        secondNumber.Should().EndWith("000004"); // 3 existing + 1 = 4
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOrder()
    {
        // Act
        var result = await _orderService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(1);
        result.OrderNumber.Should().Be("ORD-2025-000001");
        result.OrderType.Should().Be("B2C");
        result.OrderStatus.Should().Be("Pending");
        result.TotalAmount.Should().Be(1250m);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        // Act
        var result = await _orderService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.OrderNumber == "ORD-2025-000001");
        result.Should().Contain(o => o.OrderNumber == "ORD-2025-000002");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
