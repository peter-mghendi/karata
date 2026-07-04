using System.Text.Json;
using CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.Hypertable;
using Karata.Kit.Platform.Models;
using Karata.Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Karata.Platform.Data;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    private ActivityConfiguration() { }
    
    
    public static readonly ActivityConfiguration Instance = new();

    public void Configure(EntityTypeBuilder<Activity> activity)
    {
        activity.HasKey(x => new { x.Id, OriginatedAt = x.OccurredAt });

        activity.IsHypertable(x => x.OccurredAt)
            .WithChunkSkipping(x => x.OccurredAt)
            .WithChunkTimeInterval("86400000")
            .WithMigrateData();
        
        activity.Property(a => a.Actions).HasConversion<string>(
            convertToProviderExpression: list => JsonSerializer.Serialize(list),
            convertFromProviderExpression: json => JsonSerializer.Deserialize<List<ActionData>>(json) ?? new()
        );

        activity.Property(a => a.Metadata).HasConversion<string>(
            convertToProviderExpression: list => JsonSerializer.Serialize(list),
            convertFromProviderExpression: json => JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new()
        );
    }
}