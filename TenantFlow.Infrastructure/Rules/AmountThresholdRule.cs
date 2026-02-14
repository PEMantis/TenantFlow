using TenantFlow.Domain.Rules;

namespace TenantFlow.Infrastructure.Rules;

public sealed class AmountThresholdRule : ITransitionRule
{
    public string Key => "amount-under-threshold";

    public bool Evaluate(RuleContext context)
    {
        if (!context.Attributes.TryGetValue("amount", out var raw) || raw is null)
            return false;

        // Accept int/decimal/double/string
        if (!decimal.TryParse(raw.ToString(), out var amount))
            return false;

        // Tenant can pass threshold per request or default:
        decimal threshold = 1000m;
        if (context.Attributes.TryGetValue("threshold", out var tRaw) && tRaw is not null)
            decimal.TryParse(tRaw.ToString(), out threshold);

        return amount <= threshold;
    }
}
