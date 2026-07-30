using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<FoodCostSnapshot> FoodCostSnapshots { get; set; }
    public virtual DbSet<RecipeCostHistory> RecipeCostHistories { get; set; }
    public virtual DbSet<WasteEvent> WasteEvents { get; set; }
    public virtual DbSet<VarianceAlert> VarianceAlerts { get; set; }
    public virtual DbSet<FoodCostAuditEvent> FoodCostAuditEvents { get; set; }

    partial void ConfigureFoodCostModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(e =>
        {
            e.Property(x => x.YieldPercent).HasPrecision(8, 4).HasColumnName("yield_percent").HasDefaultValue(100m);
            e.Property(x => x.TargetFoodCostPercent).HasPrecision(8, 4).HasColumnName("target_food_cost_percent");
            e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        });

        modelBuilder.Entity<RecipeLine>(e =>
        {
            e.Property(x => x.WastePercent).HasPrecision(8, 4).HasColumnName("waste_percent").HasDefaultValue(0m);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(x => x.TheoreticalUnitCost).HasPrecision(18, 4).HasColumnName("theoretical_unit_cost");
            e.Property(x => x.CostSnapshotAt).HasColumnName("cost_snapshot_at");
        });

        modelBuilder.Entity<FoodCostSnapshot>(e =>
        {
            e.ToTable("food_cost_snapshots");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.PeriodStart).HasColumnName("period_start");
            e.Property(x => x.PeriodEnd).HasColumnName("period_end");
            e.Property(x => x.SalesTotal).HasPrecision(18, 2).HasColumnName("sales_total");
            e.Property(x => x.TheoreticalCogs).HasPrecision(18, 2).HasColumnName("theoretical_cogs");
            e.Property(x => x.ActualCogs).HasPrecision(18, 2).HasColumnName("actual_cogs");
            e.Property(x => x.VarianceAmount).HasPrecision(18, 2).HasColumnName("variance_amount");
            e.Property(x => x.VariancePercent).HasPrecision(8, 4).HasColumnName("variance_percent");
            e.Property(x => x.WasteCost).HasPrecision(18, 2).HasColumnName("waste_cost");
            e.Property(x => x.FoodCostPercentTheo).HasPrecision(8, 4).HasColumnName("food_cost_percent_theo");
            e.Property(x => x.FoodCostPercentActual).HasPrecision(8, 4).HasColumnName("food_cost_percent_actual");
            e.Property(x => x.GeneratedAt).HasColumnName("generated_at");
            e.Property(x => x.GeneratedByUserId).HasColumnName("generated_by_user_id");
            e.HasIndex(x => new { x.BranchId, x.PeriodStart }).HasDatabaseName("IX_food_cost_snapshots_branch_period");
        });

        modelBuilder.Entity<RecipeCostHistory>(e =>
        {
            e.ToTable("recipe_cost_histories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RecipeId).HasColumnName("recipe_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.TheoreticalCost).HasPrecision(18, 4).HasColumnName("theoretical_cost");
            e.Property(x => x.FoodCostPercent).HasPrecision(8, 4).HasColumnName("food_cost_percent");
            e.Property(x => x.MarginAmount).HasPrecision(18, 4).HasColumnName("margin_amount");
            e.Property(x => x.Source).HasMaxLength(40).HasColumnName("source");
            e.Property(x => x.RecordedAt).HasColumnName("recorded_at");
            e.HasIndex(x => new { x.ProductId, x.RecordedAt }).HasDatabaseName("IX_recipe_cost_histories_product");
        });

        modelBuilder.Entity<WasteEvent>(e =>
        {
            e.ToTable("waste_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.StationId).HasColumnName("station_id");
            e.Property(x => x.Quantity).HasPrecision(18, 4).HasColumnName("quantity");
            e.Property(x => x.UnitCost).HasPrecision(18, 4).HasColumnName("unit_cost");
            e.Property(x => x.TotalCost).HasPrecision(18, 2).HasColumnName("total_cost");
            e.Property(x => x.ReasonCode).HasConversion<string>().HasMaxLength(30).HasColumnName("reason_code");
            e.Property(x => x.ReasonNotes).HasMaxLength(500).HasColumnName("reason_notes");
            e.Property(x => x.ResponsibleUserId).HasColumnName("responsible_user_id");
            e.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
            e.Property(x => x.InventoryMovementId).HasColumnName("inventory_movement_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.BranchId, x.CreatedAt }).HasDatabaseName("IX_waste_events_branch_created");
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<VarianceAlert>(e =>
        {
            e.ToTable("variance_alerts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.AlertType).HasConversion<string>().HasMaxLength(30).HasColumnName("alert_type");
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).HasColumnName("severity");
            e.Property(x => x.Message).HasMaxLength(500).HasColumnName("message");
            e.Property(x => x.PeriodStart).HasColumnName("period_start");
            e.Property(x => x.PeriodEnd).HasColumnName("period_end");
            e.Property(x => x.VariancePercent).HasPrecision(8, 4).HasColumnName("variance_percent");
            e.Property(x => x.IsResolved).HasColumnName("is_resolved");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<FoodCostAuditEvent>(e =>
        {
            e.ToTable("food_cost_audit_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.EntityType).HasMaxLength(40).HasColumnName("entity_type");
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.EventType).HasMaxLength(80).HasColumnName("event_type");
            e.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
            e.Property(x => x.BeforeJson).HasColumnName("before_json");
            e.Property(x => x.AfterJson).HasColumnName("after_json");
            e.Property(x => x.PreviousEventHash).HasMaxLength(64).HasColumnName("previous_event_hash");
            e.Property(x => x.EventHash).HasMaxLength(64).HasColumnName("event_hash");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.CreatedAtUtc }).HasDatabaseName("IX_food_cost_audit_company");
        });
    }
}
