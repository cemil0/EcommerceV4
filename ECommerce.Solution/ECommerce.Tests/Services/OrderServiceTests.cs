using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;

namespace ECommerce.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        var stockReservationMock = new Mock<IStockReservationService>();
        var priceValidationMock = new Mock<IPriceValidationService>();
        var orderBusinessRulesMock = new Mock<IOrderBusinessRules>();
        var orderStateMachineMock = new Mock<IOrderStateMachine>();
        var loggerMock = new Mock<ILogger<OrderService>>();
        
        _orderService = new OrderService(
            _unitOfWorkMock.Object, 
            _mapperMock.Object,
            stockReservationMock.Object,
            priceValidationMock.Object,
            orderBusinessRulesMock.Object,
            orderStateMachineMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task GenerateOrderNumberAsync_ReturnsCorrectFormat()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var orderCount = 5;

        _unitOfWorkMock.Setup(u => u.Orders.GetCountForYearAsync(currentYear))
            .ReturnsAsync(orderCount);

        // Act
        var result = await _orderService.GenerateOrderNumberAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith($"ORD-{currentYear}-");
        result.Should().EndWith("000006"); // count + 1, padded to 6 digits

        _unitOfWorkMock.Verify(u => u.Orders.GetCountForYearAsync(currentYear), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOrderDto()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            OrderId = orderId,
            OrderNumber = "ORD-2025-000001",
            TotalAmount = 1000m
        };

        var orderDto = new OrderDto
        {
            OrderId = orderId,
            OrderNumber = "ORD-2025-000001",
            TotalAmount = 1000m
        };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mapperMock.Setup(m => m.Map<OrderDto>(order))
            .Returns(orderDto);

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
        result.OrderNumber.Should().Be("ORD-2025-000001");
        result.TotalAmount.Should().Be(1000m);

        _unitOfWorkMock.Verify(u => u.Orders.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WithValidNumber_ReturnsOrderDto()
    {
        // Arrange
        var orderNumber = "ORD-2025-000001";
        var order = new Order
        {
            OrderId = 1,
            OrderNumber = orderNumber,
            TotalAmount = 1000m
        };

        var orderDto = new OrderDto
        {
            OrderId = 1,
            OrderNumber = orderNumber,
            TotalAmount = 1000m
        };

        _unitOfWorkMock.Setup(u => u.Orders.GetByOrderNumberAsync(orderNumber))
            .ReturnsAsync(order);

        _mapperMock.Setup(m => m.Map<OrderDto>(order))
            .Returns(orderDto);

        // Act
        var result = await _orderService.GetByOrderNumberAsync(orderNumber);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be(orderNumber);

        _unitOfWorkMock.Verify(u => u.Orders.GetByOrderNumberAsync(orderNumber), Times.Once);
    }
}
