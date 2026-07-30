using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<ExecutiveSnapshot> ExecutiveSnapshots { get; set; }
    public virtual DbSet<BiInsight> BiInsights { get; set; }
    public virtual DbSet<BiAlert> BiAlerts { get; set; }
    public virtual DbSet<BiScore> BiScores { get; set; }
    public virtual DbSet<BiAuditEvent> BiAuditEvents { get; set; }
    public virtual DbSet<ForecastSeed> ForecastSeeds { get; set; }

    partial void ConfigureIntelligenceModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExecutiveSnapshot>(e =>
        {
            e.ToTable("executive_snapshots");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.PeriodType).HasMaxLength(20).HasColumnName("period_type");
            e.Property(x => x.SnapshotJson).HasColumnName("snapshot_json");
            e.Property(x => x.EnterpriseScore).HasPrecision(5, 2).HasColumnName("enterprise_score");
            e.Property(x => x.GeneratedAt).HasColumnName("generated_at");
            e.HasIndex(x => new { x.BranchId, x.GeneratedAt }).HasDatabaseName("IX_executive_snapshots_branch");
        });

        modelBuilder.Entity<BiInsight>(e =>
        {
            e.ToTable("bi_insights");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.InsightType).HasConversion<string>().HasMaxLength(40).HasColumnName("insight_type");
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).HasColumnName("severity");
            e.Property(x => x.Title).HasMaxLength(200).HasColumnName("title");
            e.Property(x => x.Explanation).HasMaxLength(1000).HasColumnName("explanation");
            e.Property(x => x.RecommendedAction).HasMaxLength(500).HasColumnName("recommended_action");
            e.Property(x => x.EntityType).HasMaxLength(40).HasColumnName("entity_type");
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.IsDismissed).HasColumnName("is_dismissed");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.BranchId, x.CreatedAt }).HasDatabaseName("IX_bi_insights_branch");
        });

        modelBuilder.Entity<BiAlert>(e =>
        {
            e.ToTable("bi_alerts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.AlertCode).HasMaxLength(40).HasColumnName("alert_code");
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).HasColumnName("severity");
            e.Property(x => x.Message).HasMaxLength(500).HasColumnName("message");
            e.Property(x => x.SourceModule).HasMaxLength(40).HasColumnName("source_module");
            e.Property(x => x.IsResolved).HasColumnName("is_resolved");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.BranchId, x.CreatedAt }).HasDatabaseName("IX_bi_alerts_branch");
        });

        modelBuilder.Entity<BiScore>(e =>
        {
            e.ToTable("bi_scores");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.SubjectType).HasConversion<string>().HasMaxLength(20).HasColumnName("subject_type");
            e.Property(x => x.SubjectId).HasColumnName("subject_id");
            e.Property(x => x.Score).HasPrecision(5, 2).HasColumnName("score");
            e.Property(x => x.DimensionsJson).HasColumnName("dimensions_json");
            e.Property(x => x.ComputedAt).HasColumnName("computed_at");
            e.HasIndex(x => new { x.CompanyId, x.SubjectType, x.SubjectId }).HasDatabaseName("IX_bi_scores_subject");
        });

        modelBuilder.Entity<BiAuditEvent>(e =>
        {
            e.ToTable("bi_audit_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
            e.Property(x => x.QueryName).HasMaxLength(80).HasColumnName("query_name");
            e.Property(x => x.FiltersJson).HasColumnName("filters_json");
            e.Property(x => x.DurationMs).HasColumnName("duration_ms");
            e.Property(x => x.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            e.Property(x => x.EventHash).HasMaxLength(64).HasColumnName("event_hash");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.Entity<ForecastSeed>(e =>
        {
            e.ToTable("forecast_seeds");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.MetricCode).HasMaxLength(40).HasColumnName("metric_code");
            e.Property(x => x.AsOfDate).HasColumnName("as_of_date");
            e.Property(x => x.Value).HasPrecision(18, 4).HasColumnName("value");
            e.Property(x => x.Source).HasMaxLength(40).HasColumnName("source");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.BranchId, x.MetricCode, x.AsOfDate }).HasDatabaseName("IX_forecast_seeds_metric");
        });
    }
}
