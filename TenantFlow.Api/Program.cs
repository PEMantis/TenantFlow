using Microsoft.EntityFrameworkCore;
using TenantFlow.Application.Abstractions;
using TenantFlow.Application.Dtos;
using TenantFlow.Application.Services;
using TenantFlow.Domain.Tenancy;
using TenantFlow.Domain.Workflows;
using TenantFlow.Infrastructure.Persistence;
using TenantFlow.Infrastructure.Repositories;
using TenantFlow.Infrastructure.Rules;
using TenantFlow.Domain.Rules;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db
builder.Services.AddDbContext<TenantFlowDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("TenantFlow") ?? "Data Source=tenantflow.db");
});

// App services
builder.Services.AddScoped<WorkflowEngineService>();

// Repos
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();

// Rules + registry
builder.Services.AddSingleton<ITransitionRule, AlwaysAllowRule>();
builder.Services.AddSingleton<ITransitionRule, AmountThresholdRule>();
builder.Services.AddSingleton<IRuleRegistry, RuleRegistry>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/workflow-instances/{instanceId:guid}", async (
    HttpRequest req,
    IWorkflowInstanceRepository instances,
    Guid instanceId,
    CancellationToken ct) =>
{
    var tenantId = GetTenantId(req);

    var instance = await instances.GetAsync(tenantId, instanceId, ct);
    if (instance is null) return Results.NotFound();

    return Results.Ok(new
    {
        instance.Id,
        instance.TenantId,
        instance.DefinitionId,
        instance.CurrentState,
        instance.CreatedAt,
        History = instance.History
            .OrderBy(h => h.ExecutedAt)
            .Select(h => new
            {
                h.FromState,
                h.ToState,
                h.ExecutedAt,
                h.ExecutedBy,
                h.RuleKey,
                h.RulePassed,
                h.Notes
            })
    });
});

// Basic tenant header helper
static Guid GetTenantId(HttpRequest req)
{
    if (!req.Headers.TryGetValue("X-Tenant-Id", out var v) || string.IsNullOrWhiteSpace(v))
        throw new ArgumentException("Missing X-Tenant-Id header.");

    if (!Guid.TryParse(v.ToString(), out var tenantId))
        throw new ArgumentException("Invalid X-Tenant-Id header. Must be a GUID.");

    return tenantId;
}

// --- Endpoints ---

app.MapPost("/tenants", async (ITenantRepository tenants, string name, CancellationToken ct) =>
{
    var tenant = new Tenant(Guid.NewGuid(), name);
    await tenants.AddAsync(tenant, ct);
    return Results.Created($"/tenants/{tenant.Id}", tenant);
});

app.MapPost("/workflow-definitions", async (
    HttpRequest req,
    IWorkflowDefinitionRepository defs,
    string name,
    string initialState,
    string[] otherStates,
    (string From, string To, string? RuleKey)[] transitions,
    CancellationToken ct) =>
{
    var tenantId = GetTenantId(req);

    var states = new List<string> { initialState };
    states.AddRange(otherStates ?? Array.Empty<string>());

    var domainTransitions = transitions.Select(t => new WorkflowTransition(t.From, t.To, t.RuleKey)).ToList();

    var def = new WorkflowDefinition(Guid.NewGuid(), tenantId, name, states, domainTransitions);
    await defs.AddAsync(def, ct);

    return Results.Created($"/workflow-definitions/{def.Id}", def.Id);
});

app.MapPost("/workflow-definitions/{definitionId:guid}/publish", async (
    HttpRequest req,
    IWorkflowDefinitionRepository defs,
    Guid definitionId,
    CancellationToken ct) =>
{
    var tenantId = GetTenantId(req);
    await defs.PublishAsync(tenantId, definitionId, ct);
    return Results.Ok();
});

app.MapPost("/workflow-instances", async (
    HttpRequest req,
    IWorkflowDefinitionRepository defs,
    IWorkflowInstanceRepository instances,
    Guid definitionId,
    string initialState,
    CancellationToken ct) =>
{
    var tenantId = GetTenantId(req);

    var def = await defs.GetAsync(tenantId, definitionId, ct);
    if (def is null) return Results.NotFound("Definition not found.");
    if (!def.IsPublished) return Results.BadRequest("Definition must be published.");

    if (!def.HasState(initialState)) return Results.BadRequest("Invalid initial state.");

    var instance = new WorkflowInstance(Guid.NewGuid(), tenantId, definitionId, initialState);
    await instances.AddAsync(instance, ct);

    return Results.Created($"/workflow-instances/{instance.Id}", instance.Id);
});

app.MapPost("/workflow-instances/{instanceId:guid}/transition", async (
    HttpRequest req,
    WorkflowEngineService engine,
    Guid instanceId,
    TransitionRequestDto dto,
    CancellationToken ct) =>
{
    var tenantId = GetTenantId(req);

    var result = await engine.TransitionAsync(
        tenantId: tenantId,
        instanceId: instanceId,
        toState: dto.ToState,
        executedBy: dto.ExecutedBy,
        attributes: dto.Attributes,
        ct: ct
    );

    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.Run();
