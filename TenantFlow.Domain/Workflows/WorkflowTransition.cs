namespace TenantFlow.Domain.Workflows;

public sealed record WorkflowTransition(
    string FromState,
    string ToState,
    string? RuleKey = null
);
