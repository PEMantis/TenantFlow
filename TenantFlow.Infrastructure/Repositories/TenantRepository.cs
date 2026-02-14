using Microsoft.EntityFrameworkCore;
using TenantFlow.Application.Abstractions;
using TenantFlow.Domain.Tenancy;
using TenantFlow.Infrastructure.Persistence;

namespace TenantFlow.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly TenantFlowDbContext _db;

    public TenantRepository(TenantFlowDbContext db) => _db = db;

    public Task<Tenant?> GetAsync(Guid id, CancellationToken ct)
        => _db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct)
    {
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(ct);
    }
}
