using TenantFlow.Domain.Common;

namespace TenantFlow.Domain.Workflows;

public sealed class WorkflowInstance
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid DefinitionId { get; private set; }

    public string CurrentState { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<WorkflowTransitionHistory> History => _history;
    private readonly List<WorkflowTransitionHistory> _history = new();

    private WorkflowInstance() { } // EF

    public WorkflowInstance(Guid id, Guid tenantId, Guid definitionId, string initialState)
    {
        Id = id;
        TenantId = tenantId;
        DefinitionId = definitionId;
        CurrentState = initialState;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Result ApplyTransition(string toState, string? executedBy, string? ruleKey, bool rulePassed, string? ruleNotes = null)
    {
        if (string.IsNullOrWhiteSpace(toState)) return Result.Fail("Target state is required.");

        var from = CurrentState;
        CurrentState = toState;

        _history.Add(new WorkflowTransitionHistory(
            Guid.NewGuid(),
            Id,
            from,
            toState,
            DateTimeOffset.UtcNow,
            executedBy ?? "system",
            ruleKey,
            rulePassed,
            ruleNotes
        ));

        return Result.Success();
    }
}
