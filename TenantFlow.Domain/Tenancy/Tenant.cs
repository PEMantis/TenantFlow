namespace TenantFlow.Domain.Tenancy;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Tenant() { } // EF
    public Tenant(Guid id, string name)
    {
        Id = id;
        Name = name.Trim();
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Tenant name is required.");
    }
}
