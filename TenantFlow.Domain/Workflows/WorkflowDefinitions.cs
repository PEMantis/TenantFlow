namespace TenantFlow.Domain.Workflows;

public sealed class WorkflowDefinition
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Keep as strings for persistence simplicity in PoC
    public IReadOnlyList<string> States => _states;
    private readonly List<string> _states = new();

    public IReadOnlyList<WorkflowTransition> Transitions => _transitions;
    private readonly List<WorkflowTransition> _transitions = new();

    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private WorkflowDefinition() { } // EF

    public WorkflowDefinition(Guid id, Guid tenantId, string name, IEnumerable<string> states, IEnumerable<WorkflowTransition> transitions)
    {
        Id = id;
        TenantId = tenantId;
        Name = name.Trim();
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Workflow name is required.");

        _states.AddRange(states.Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
        if (_states.Count < 2) throw new ArgumentException("Workflow must have at least two states.");

        _transitions.AddRange(transitions);
        if (_transitions.Count == 0) throw new ArgumentException("Workflow must define transitions.");

        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish() => IsPublished = true;

    public bool HasState(string state) => _states.Contains(state);
}
