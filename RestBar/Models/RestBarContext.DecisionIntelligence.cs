using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<DiDecisionRecord> DiDecisionRecords { get; set; }
    public virtual DbSet<DiManualEvent> DiManualEvents { get; set; }
    public virtual DbSet<DiForecastRun> DiForecastRuns { get; set; }

    partial void ConfigureDecisionIntelligenceModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiDecisionRecord>(e =>
        {
            e.ToTable("di_decision_records");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.RecommendationCode).HasMaxLength(40).HasColumnName("recommendation_code");
            e.Property(x => x.Category).HasMaxLength(40).HasColumnName("category");
            e.Property(x => x.Observation).HasMaxLength(2000).HasColumnName("observation");
            e.Property(x => x.Evidence).HasMaxLength(2000).HasColumnName("evidence");
            e.Property(x => x.RecommendedAction).HasMaxLength(1000).HasColumnName("recommended_action");
            e.Property(x => x.ExpectedImpact).HasMaxLength(1000).HasColumnName("expected_impact");
            e.Property(x => x.ExpectedImpactValue).HasPrecision(18, 4).HasColumnName("expected_impact_value");
            e.Property(x => x.ActualImpactValue).HasPrecision(18, 4).HasColumnName("actual_impact_value");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(x => x.AssignedToUserId).HasColumnName("assigned_to_user_id");
            e.Property(x => x.Comment).HasMaxLength(1000).HasColumnName("comment");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            e.Property(x => x.DueAtUtc).HasColumnName("due_at_utc");
            e.Property(x => x.VerifiedAtUtc).HasColumnName("verified_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.Status, x.CreatedAtUtc }).HasDatabaseName("IX_di_decisions_company_status");
        });

        modelBuilder.Entity<DiManualEvent>(e =>
        {
            e.ToTable("di_manual_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.EventDate).HasColumnName("event_date");
            e.Property(x => x.EventType).HasMaxLength(40).HasColumnName("event_type");
            e.Property(x => x.Title).HasMaxLength(200).HasColumnName("title");
            e.Property(x => x.Notes).HasMaxLength(500).HasColumnName("notes");
            e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.EventDate }).HasDatabaseName("IX_di_manual_events_date");
        });

        modelBuilder.Entity<DiForecastRun>(e =>
        {
            e.ToTable("di_forecast_runs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.MetricCode).HasMaxLength(40).HasColumnName("metric_code");
            e.Property(x => x.ModelId).HasMaxLength(40).HasColumnName("model_id");
            e.Property(x => x.HorizonDays).HasColumnName("horizon_days");
            e.Property(x => x.HistoryPoints).HasColumnName("history_points");
            e.Property(x => x.Mae).HasPrecision(18, 4).HasColumnName("mae");
            e.Property(x => x.Mape).HasPrecision(18, 4).HasColumnName("mape");
            e.Property(x => x.Rmse).HasPrecision(18, 4).HasColumnName("rmse");
            e.Property(x => x.BeatsNaive).HasColumnName("beats_naive");
            e.Property(x => x.Confidence).HasMaxLength(20).HasColumnName("confidence");
            e.Property(x => x.ForecastJson).HasColumnName("forecast_json");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.BranchId, x.CreatedAtUtc }).HasDatabaseName("IX_di_forecast_runs_company");
        });
    }
}
