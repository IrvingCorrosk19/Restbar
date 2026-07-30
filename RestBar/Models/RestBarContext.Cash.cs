using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<CashRegister> CashRegisters { get; set; }
    public virtual DbSet<CashSession> CashSessions { get; set; }
    public virtual DbSet<CashMovement> CashMovements { get; set; }
    public virtual DbSet<CashCount> CashCounts { get; set; }
    public virtual DbSet<CashCountLine> CashCountLines { get; set; }
    public virtual DbSet<CashApproval> CashApprovals { get; set; }
    public virtual DbSet<CashIncident> CashIncidents { get; set; }
    public virtual DbSet<CashAuditEvent> CashAuditEvents { get; set; }
    public virtual DbSet<CashZReport> CashZReports { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CashRegister>(entity =>
        {
            entity.ToTable("cash_registers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Code).HasMaxLength(20).HasColumnName("code");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.RegisterType).HasColumnName("register_type").HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.DefaultOpeningFloat).HasPrecision(18, 2).HasColumnName("default_opening_float");
            entity.Property(e => e.RequiresBlindClose).HasColumnName("requires_blind_close").HasDefaultValue(false);
            entity.Property(e => e.VarianceThresholdAmount).HasPrecision(18, 2).HasColumnName("variance_threshold_amount");
            entity.Property(e => e.VarianceThresholdPercent).HasPrecision(8, 4).HasColumnName("variance_threshold_percent");
            entity.Property(e => e.MaxPaidOutWithoutApproval).HasPrecision(18, 2).HasColumnName("max_paid_out_without_approval");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.BusinessDayCutoffHour).HasColumnName("business_day_cutoff_hour").HasDefaultValue(4);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedBy).HasMaxLength(256).HasColumnName("created_by");
            entity.Property(e => e.UpdatedBy).HasMaxLength(256).HasColumnName("updated_by");
            entity.HasIndex(e => new { e.BranchId, e.Code }).IsUnique().HasDatabaseName("UX_cash_registers_branch_code");
            entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId);
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
            entity.HasOne(e => e.Station).WithMany().HasForeignKey(e => e.StationId);
        });

        modelBuilder.Entity<CashSession>(entity =>
        {
            entity.ToTable("cash_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CashRegisterId).HasColumnName("cash_register_id");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.SessionNumber).HasColumnName("session_number");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.OpenedAt).HasColumnName("opened_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ClosedAt).HasColumnName("closed_at");
            entity.Property(e => e.OpenedByUserId).HasColumnName("opened_by_user_id");
            entity.Property(e => e.ClosedByUserId).HasColumnName("closed_by_user_id");
            entity.Property(e => e.SupervisorUserId).HasColumnName("supervisor_user_id");
            entity.Property(e => e.ManagerUserId).HasColumnName("manager_user_id");
            entity.Property(e => e.OpeningFloatDeclared).HasPrecision(18, 2).HasColumnName("opening_float_declared");
            entity.Property(e => e.ExpectedCash).HasPrecision(18, 2).HasColumnName("expected_cash");
            entity.Property(e => e.CountedCash).HasPrecision(18, 2).HasColumnName("counted_cash");
            entity.Property(e => e.Variance).HasPrecision(18, 2).HasColumnName("variance");
            entity.Property(e => e.ExpectedCard).HasPrecision(18, 2).HasColumnName("expected_card");
            entity.Property(e => e.ExpectedDigital).HasPrecision(18, 2).HasColumnName("expected_digital");
            entity.Property(e => e.TotalSales).HasPrecision(18, 2).HasColumnName("total_sales");
            entity.Property(e => e.TotalRefunds).HasPrecision(18, 2).HasColumnName("total_refunds");
            entity.Property(e => e.TotalTips).HasPrecision(18, 2).HasColumnName("total_tips");
            entity.Property(e => e.TotalPaidIn).HasPrecision(18, 2).HasColumnName("total_paid_in");
            entity.Property(e => e.TotalPaidOut).HasPrecision(18, 2).HasColumnName("total_paid_out");
            entity.Property(e => e.BlindCloseEnabled).HasColumnName("blind_close_enabled").HasDefaultValue(false);
            entity.Property(e => e.CloseNotes).HasMaxLength(1000).HasColumnName("close_notes");
            entity.Property(e => e.ReopenedFromSessionId).HasColumnName("reopened_from_session_id");
            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken()
                .HasColumnName("row_version")
                .HasDefaultValueSql("decode(md5(random()::text || clock_timestamp()::text), 'hex')");
            entity.HasIndex(e => new { e.CashRegisterId, e.Status }).HasDatabaseName("IX_cash_sessions_register_status");
            entity.HasIndex(e => new { e.BranchId, e.OpenedAt }).HasDatabaseName("IX_cash_sessions_branch_opened");
            entity.HasOne(e => e.CashRegister).WithMany(r => r.Sessions).HasForeignKey(e => e.CashRegisterId);
            entity.HasOne(e => e.Shift).WithMany().HasForeignKey(e => e.ShiftId);
            entity.HasOne(e => e.OpenedByUser).WithMany().HasForeignKey(e => e.OpenedByUserId);
        });

        modelBuilder.Entity<CashMovement>(entity =>
        {
            entity.ToTable("cash_movements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.Property(e => e.MovementType).HasColumnName("movement_type").HasConversion<string>().HasMaxLength(40);
            entity.Property(e => e.Direction).HasColumnName("direction").HasConversion<string>().HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 2).HasColumnName("amount");
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).HasColumnName("currency_code").HasDefaultValue("USD");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentRefundId).HasColumnName("payment_refund_id");
            entity.Property(e => e.RelatedMovementId).HasColumnName("related_movement_id");
            entity.Property(e => e.ReasonCode).HasMaxLength(50).HasColumnName("reason_code");
            entity.Property(e => e.Comments).HasMaxLength(500).HasColumnName("comments");
            entity.Property(e => e.PerformedByUserId).HasColumnName("performed_by_user_id");
            entity.Property(e => e.AuthorizedByUserId).HasColumnName("authorized_by_user_id");
            entity.Property(e => e.SequenceNumber).HasColumnName("sequence_number");
            entity.Property(e => e.PreviousHash).HasMaxLength(64).HasColumnName("previous_hash");
            entity.Property(e => e.RecordHash).HasMaxLength(64).HasColumnName("record_hash");
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100).HasColumnName("idempotency_key");
            entity.Property(e => e.Source).HasColumnName("source").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.DeviceId).HasMaxLength(100).HasColumnName("device_id");
            entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.AffectsCashDrawer).HasColumnName("affects_cash_drawer").HasDefaultValue(true);
            entity.HasIndex(e => new { e.CashSessionId, e.SequenceNumber }).IsUnique().HasDatabaseName("UX_cash_movements_session_seq");
            entity.HasIndex(e => e.IdempotencyKey).IsUnique().HasDatabaseName("UX_cash_movements_idempotency")
                .HasFilter("\"idempotency_key\" IS NOT NULL");
            entity.HasIndex(e => new { e.CashSessionId, e.CreatedAtUtc }).HasDatabaseName("IX_cash_movements_session_created");
            entity.HasOne(e => e.CashSession).WithMany(s => s.Movements).HasForeignKey(e => e.CashSessionId);
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId);
        });

        modelBuilder.Entity<CashCount>(entity =>
        {
            entity.ToTable("cash_counts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.Property(e => e.CountType).HasColumnName("count_type").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CountedAtUtc).HasColumnName("counted_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CountedByUserId).HasColumnName("counted_by_user_id");
            entity.Property(e => e.WitnessUserId).HasColumnName("witness_user_id");
            entity.Property(e => e.TotalCounted).HasPrecision(18, 2).HasColumnName("total_counted");
            entity.Property(e => e.IsBlind).HasColumnName("is_blind").HasDefaultValue(false);
            entity.HasOne(e => e.CashSession).WithMany(s => s.Counts).HasForeignKey(e => e.CashSessionId);
        });

        modelBuilder.Entity<CashCountLine>(entity =>
        {
            entity.ToTable("cash_count_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CashCountId).HasColumnName("cash_count_id");
            entity.Property(e => e.DenominationValue).HasPrecision(18, 2).HasColumnName("denomination_value");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Subtotal).HasPrecision(18, 2).HasColumnName("subtotal");
            entity.HasOne(e => e.CashCount).WithMany(c => c.Lines).HasForeignKey(e => e.CashCountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CashApproval>(entity =>
        {
            entity.ToTable("cash_approvals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.Property(e => e.CashMovementId).HasColumnName("cash_movement_id");
            entity.Property(e => e.ApprovalType).HasColumnName("approval_type").HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.RequestedByUserId).HasColumnName("requested_by_user_id");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(e => e.ThresholdAmount).HasPrecision(18, 2).HasColumnName("threshold_amount");
            entity.Property(e => e.ActualAmount).HasPrecision(18, 2).HasColumnName("actual_amount");
            entity.Property(e => e.Reason).HasMaxLength(500).HasColumnName("reason");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.HasOne(e => e.CashSession).WithMany(s => s.Approvals).HasForeignKey(e => e.CashSessionId);
        });

        modelBuilder.Entity<CashIncident>(entity =>
        {
            entity.ToTable("cash_incidents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.Property(e => e.IncidentType).HasColumnName("incident_type").HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Severity).HasColumnName("severity").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(1000).HasColumnName("description");
            entity.Property(e => e.ResolvedByUserId).HasColumnName("resolved_by_user_id");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000).HasColumnName("resolution_notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.CashSession).WithMany().HasForeignKey(e => e.CashSessionId);
        });

        modelBuilder.Entity<CashAuditEvent>(entity =>
        {
            entity.ToTable("cash_audit_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.Property(e => e.CashMovementId).HasColumnName("cash_movement_id");
            entity.Property(e => e.EventType).HasMaxLength(80).HasColumnName("event_type");
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.ActorRole).HasMaxLength(50).HasColumnName("actor_role");
            entity.Property(e => e.BeforeJson).HasColumnName("before_json");
            entity.Property(e => e.AfterJson).HasColumnName("after_json");
            entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.DeviceId).HasMaxLength(100).HasColumnName("device_id");
            entity.Property(e => e.PreviousEventHash).HasMaxLength(64).HasColumnName("previous_event_hash");
            entity.Property(e => e.EventHash).HasMaxLength(64).HasColumnName("event_hash");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => new { e.CashSessionId, e.CreatedAtUtc }).HasDatabaseName("IX_cash_audit_session_created");
        });

        modelBuilder.Entity<CashZReport>(entity =>
        {
            entity.ToTable("cash_z_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.ReportJson).HasColumnName("report_json");
            entity.Property(e => e.GeneratedAtUtc).HasColumnName("generated_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.GeneratedByUserId).HasColumnName("generated_by_user_id");
            entity.Property(e => e.IntegrityHash).HasMaxLength(64).HasColumnName("integrity_hash");
            entity.HasIndex(e => e.CashSessionId).IsUnique().HasDatabaseName("UX_cash_z_reports_session");
            entity.HasOne(e => e.CashSession).WithMany().HasForeignKey(e => e.CashSessionId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.HasOne(e => e.CashSession).WithMany().HasForeignKey(e => e.CashSessionId);
            entity.HasIndex(e => e.CashSessionId).HasDatabaseName("IX_payments_cash_session");
        });

        modelBuilder.Entity<PaymentRefund>(entity =>
        {
            entity.Property(e => e.CashSessionId).HasColumnName("cash_session_id");
            entity.HasOne(e => e.CashSession).WithMany().HasForeignKey(e => e.CashSessionId);
        });

        ConfigureProcurementModel(modelBuilder);
        ConfigureFoodCostModel(modelBuilder);
        ConfigureIntelligenceModel(modelBuilder);
        ConfigureCopilotModel(modelBuilder);
    }

    partial void ConfigureProcurementModel(ModelBuilder modelBuilder);
    partial void ConfigureFoodCostModel(ModelBuilder modelBuilder);
    partial void ConfigureIntelligenceModel(ModelBuilder modelBuilder);
    partial void ConfigureCopilotModel(ModelBuilder modelBuilder);
}
