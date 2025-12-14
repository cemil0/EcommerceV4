using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class AdminAuditLogRepository : Repository<AdminAuditLog>, IAdminAuditLogRepository
{
    public AdminAuditLogRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AdminAuditLog>> GetByAdminIdAsync(string adminId)
    {
        return await _context.Set<AdminAuditLog>()
            .Where(x => x.AdminUserId == adminId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AdminAuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        return await _context.Set<AdminAuditLog>()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }
}
