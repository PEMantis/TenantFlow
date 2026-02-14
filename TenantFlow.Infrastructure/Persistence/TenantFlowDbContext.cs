using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TenantFlow.Domain.Tenancy;
using TenantFlow.Domain.Workflows;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace TenantFlow.Infrastructure.Persistence;

public sealed class TenantFlowDbContext : DbContext
{
    public TenantFlowDbContext(DbContextOptions<TenantFlowDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<WorkflowTransition>();
        modelBuilder.Ignore<WorkflowTransitionHistory>();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var statesComparer = new ValueComparer<List<string>>(
        (a, b) => a!.SequenceEqual(b!),
        v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
        v => v.ToList()
);
        var transitionsComparer = new ValueComparer<List<WorkflowTransition>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            v => v.ToList()
        );

        var historyComparer = new ValueComparer<List<WorkflowTransitionHistory>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            v => v.ToList()
        );

        // Tenant
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("Tenants");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.HasIndex(x => x.Name);
        });

        // WorkflowDefinition
        modelBuilder.Entity<WorkflowDefinition>(b =>
        {
            b.ToTable("WorkflowDefinitions");
            b.HasKey(x => x.Id);

            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.IsPublished).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            // Persist backing fields as JSON
            b.Property<List<string>>("_states")
                .HasColumnName("StatesJson")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
                )
                .IsRequired();

            b.Property<List<WorkflowTransition>>("_transitions")
                .HasColumnName("TransitionsJson")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<WorkflowTransition>>(v, jsonOptions) ?? new List<WorkflowTransition>()
                )
                .IsRequired();
           
            b.Property<List<string>>("_states")
                .HasColumnName("StatesJson")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
                )
                .Metadata.SetValueComparer(statesComparer);

            b.Property<List<WorkflowTransition>>("_transitions")
                .HasColumnName("TransitionsJson")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<WorkflowTransition>>(v, jsonOptions) ?? new List<WorkflowTransition>()
                )
                .Metadata.SetValueComparer(transitionsComparer);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.Name);
        });

        // WorkflowInstance
        modelBuilder.Entity<WorkflowInstance>(b =>
        {
            b.ToTable("WorkflowInstances");
            b.HasKey(x => x.Id);

            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.DefinitionId).IsRequired();
            b.Property(x => x.CurrentState).IsRequired().HasMaxLength(100);
            b.Property(x => x.CreatedAt).IsRequired();

            // Persist history as JSON
            b.Property<List<WorkflowTransitionHistory>>("_history")
                .HasColumnName("HistoryJson")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<WorkflowTransitionHistory>>(v, jsonOptions) ?? new List<WorkflowTransitionHistory>()
                )
                .IsRequired();
         
            b.Property<List<WorkflowTransitionHistory>>("_history")
                .HasColumnName("HistoryJson")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<WorkflowTransitionHistory>>(v, jsonOptions) ?? new List<WorkflowTransitionHistory>()
                )
                .Metadata.SetValueComparer(historyComparer);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.DefinitionId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
