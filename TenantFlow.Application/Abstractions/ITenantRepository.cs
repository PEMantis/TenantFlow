using TenantFlow.Domain.Tenancy;

namespace TenantFlow.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(Tenant tenant, CancellationToken ct);
}
