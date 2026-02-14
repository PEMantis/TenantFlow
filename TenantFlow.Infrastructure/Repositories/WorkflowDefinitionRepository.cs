using Microsoft.EntityFrameworkCore;
using TenantFlow.Application.Abstractions;
using TenantFlow.Domain.Workflows;
using TenantFlow.Infrastructure.Persistence;

namespace TenantFlow.Infrastructure.Repositories;

public sealed class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly TenantFlowDbContext _db;

    public WorkflowDefinitionRepository(TenantFlowDbContext db) => _db = db;

    public Task<WorkflowDefinition?> GetAsync(Guid tenantId, Guid definitionId, CancellationToken ct)
        => _db.WorkflowDefinitions
            .FirstOrDefaultAsync(x => EF.Property<Guid>(x, "TenantId") == tenantId && x.Id == definitionId, ct);

    public async Task AddAsync(WorkflowDefinition definition, CancellationToken ct)
    {
        _db.WorkflowDefinitions.Add(definition);
        await _db.SaveChangesAsync(ct);
    }

    public async Task PublishAsync(Guid tenantId, Guid definitionId, CancellationToken ct)
    {
        var def = await GetAsync(tenantId, definitionId, ct);
        if (def is null) return;

        def.Publish();
        await _db.SaveChangesAsync(ct);
    }
}
