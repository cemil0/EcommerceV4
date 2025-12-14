using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IAdminAuditLogRepository : IRepository<AdminAuditLog>
{
    Task<IEnumerable<AdminAuditLog>> GetByAdminIdAsync(string adminId);
    Task<IEnumerable<AdminAuditLog>> GetByEntityAsync(string entityType, string entityId);
}
