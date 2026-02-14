using TenantFlow.Domain.Rules;

namespace TenantFlow.Infrastructure.Rules;

public sealed class AlwaysAllowRule : ITransitionRule
{
    public string Key => "always-allow";
    public bool Evaluate(RuleContext context) => true;
}
