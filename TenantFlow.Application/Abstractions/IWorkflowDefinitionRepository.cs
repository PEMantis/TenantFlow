using TenantFlow.Domain.Workflows;

namespace TenantFlow.Application.Abstractions;

public interface IWorkflowDefinitionRepository
{
    Task<WorkflowDefinition?> GetAsync(Guid tenantId, Guid definitionId, CancellationToken ct);
    Task AddAsync(WorkflowDefinition definition, CancellationToken ct);
    Task PublishAsync(Guid tenantId, Guid definitionId, CancellationToken ct);
}
