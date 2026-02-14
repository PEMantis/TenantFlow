using Microsoft.EntityFrameworkCore;
using TenantFlow.Application.Abstractions;
using TenantFlow.Domain.Workflows;
using TenantFlow.Infrastructure.Persistence;

namespace TenantFlow.Infrastructure.Repositories;

public sealed class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly TenantFlowDbContext _db;

    public WorkflowInstanceRepository(TenantFlowDbContext db) => _db = db;

    public Task<WorkflowInstance?> GetAsync(Guid tenantId, Guid instanceId, CancellationToken ct)
        => _db.WorkflowInstances
            .FirstOrDefaultAsync(x => EF.Property<Guid>(x, "TenantId") == tenantId && x.Id == instanceId, ct);

    public async Task AddAsync(WorkflowInstance instance, CancellationToken ct)
    {
        _db.WorkflowInstances.Add(instance);
        await _db.SaveChangesAsync(ct);
    }

    public Task SaveAsync(WorkflowInstance instance, CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
