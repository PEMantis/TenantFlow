namespace TenantFlow.Domain.Workflows;

public sealed record WorkflowTransitionHistory(
    Guid Id,
    Guid InstanceId,
    string FromState,
    string ToState,
    DateTimeOffset ExecutedAt,
    string ExecutedBy,
    string? RuleKey,
    bool RulePassed,
    string? Notes
);
