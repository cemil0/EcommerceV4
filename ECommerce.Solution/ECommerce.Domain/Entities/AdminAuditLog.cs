using System;

namespace ECommerce.Domain.Entities;

public class AdminAuditLog
{
    public int Id { get; set; }
    public string? AdminUserId { get; set; }
    public string? AdminEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
