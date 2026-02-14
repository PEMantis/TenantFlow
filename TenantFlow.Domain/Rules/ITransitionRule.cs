namespace TenantFlow.Domain.Rules;

public interface ITransitionRule
{
    string Key { get; }
    bool Evaluate(RuleContext context);
}

public sealed record RuleContext(
    Guid TenantId,
    Guid WorkflowDefinitionId,
    Guid WorkflowInstanceId,
    Dictionary<string, object?> Attributes
);
