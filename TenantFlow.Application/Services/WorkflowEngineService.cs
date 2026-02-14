using TenantFlow.Application.Abstractions;
using TenantFlow.Domain.Common;
using TenantFlow.Domain.Rules;

namespace TenantFlow.Application.Services;

public sealed class WorkflowEngineService
{
    private readonly IWorkflowDefinitionRepository _defs;
    private readonly IWorkflowInstanceRepository _instances;
    private readonly IRuleRegistry _rules;

    public WorkflowEngineService(
        IWorkflowDefinitionRepository defs,
        IWorkflowInstanceRepository instances,
        IRuleRegistry rules)
    {
        _defs = defs;
        _instances = instances;
        _rules = rules;
    }

    public async Task<Result> TransitionAsync(
        Guid tenantId,
        Guid instanceId,
        string toState,
        string executedBy,
        Dictionary<string, object?> attributes,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(toState))
            return Result.Fail("Target state is required.");

        var instance = await _instances.GetAsync(tenantId, instanceId, ct);
        if (instance is null) return Result.Fail("Workflow instance not found.");

        var def = await _defs.GetAsync(tenantId, instance.DefinitionId, ct);
        if (def is null) return Result.Fail("Workflow definition not found.");
        if (!def.IsPublished) return Result.Fail("Workflow definition is not published.");

        var transition = def.Transitions.FirstOrDefault(t =>
            string.Equals(t.FromState, instance.CurrentState, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(t.ToState, toState, StringComparison.OrdinalIgnoreCase)
        );

        if (transition is null)
            return Result.Fail($"Transition not allowed: {instance.CurrentState} → {toState}.");

        // Evaluate rule (if any)
        if (!string.IsNullOrWhiteSpace(transition.RuleKey))
        {
            var rule = _rules.Resolve(transition.RuleKey);
            if (rule is null)
                return Result.Fail($"Rule not registered: {transition.RuleKey}.");

            var passed = rule.Evaluate(new RuleContext(
                tenantId,
                def.Id,
                instance.Id,
                attributes
            ));

            if (!passed)
                return Result.Fail("Transition rejected by rule.");
        }

        // Apply and persist
        var apply = instance.ApplyTransition(
            toState: toState,
            executedBy: executedBy,
            ruleKey: transition.RuleKey,
            rulePassed: true,
            ruleNotes: null
        );

        if (!apply.IsSuccess) return apply;

        await _instances.SaveAsync(instance, ct);
        return Result.Success();
    }
}
