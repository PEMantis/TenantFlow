using TenantFlow.Application.Abstractions;
using TenantFlow.Domain.Rules;

namespace TenantFlow.Infrastructure.Rules;

public sealed class RuleRegistry : IRuleRegistry
{
    private readonly Dictionary<string, ITransitionRule> _rules;

    public RuleRegistry(IEnumerable<ITransitionRule> rules)
    {
        _rules = rules.ToDictionary(r => r.Key, StringComparer.OrdinalIgnoreCase);
    }

    public ITransitionRule? Resolve(string key)
        => _rules.TryGetValue(key, out var rule) ? rule : null;
}
