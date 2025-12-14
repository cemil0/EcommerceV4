using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.Admin;

// Admin Dashboard
public class AdminDashboardDto
{
    public DashboardSummary Summary { get; set; } = new();
    public List<RecentOrderSummary> RecentOrders { get; set; } = new();
}

public class DashboardSummary
{
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingOrders { get; set; }
}

public class RecentOrderSummary
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Admin User Management
public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AssignRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

// Sales Chart
public class SalesChartPointDto
{
    public string Date { get; set; } = string.Empty; // Format: dd MMM (e.g., 12 Dec)
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class SalesChartDto
{
    public List<SalesChartPointDto> Data { get; set; } = new();
    public decimal TotalRevenueInPeriod { get; set; }
    public int TotalOrdersInPeriod { get; set; }
}

public class TopSellingProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}
