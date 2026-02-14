namespace TenantFlow.Domain.Workflows;

public sealed record WorkflowState(string Value)
{
    public override string ToString() => Value;
}
