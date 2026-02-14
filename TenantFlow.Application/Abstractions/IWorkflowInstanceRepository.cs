using TenantFlow.Domain.Workflows;

namespace TenantFlow.Application.Abstractions;

public interface IWorkflowInstanceRepository
{
    Task<WorkflowInstance?> GetAsync(Guid tenantId, Guid instanceId, CancellationToken ct);
    Task AddAsync(WorkflowInstance instance, CancellationToken ct);
    Task SaveAsync(WorkflowInstance instance, CancellationToken ct);
}
