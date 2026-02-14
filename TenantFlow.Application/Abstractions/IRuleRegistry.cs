using TenantFlow.Domain.Rules;

namespace TenantFlow.Application.Abstractions;

public interface IRuleRegistry
{
    ITransitionRule? Resolve(string key);
}
