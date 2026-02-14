namespace TenantFlow.Application.Dtos;

public sealed class TransitionRequestDto
{
    public string ToState { get; set; } = string.Empty;
    public string ExecutedBy { get; set; } = "system";

    // simple extensibility point for rules
    public Dictionary<string, object?> Attributes { get; set; } = new();
}
