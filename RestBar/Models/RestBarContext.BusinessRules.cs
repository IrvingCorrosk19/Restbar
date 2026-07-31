using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<BrRule> BrRules { get; set; }
    public virtual DbSet<BrRuleVersion> BrRuleVersions { get; set; }
    public virtual DbSet<BrRuleCondition> BrRuleConditions { get; set; }
    public virtual DbSet<BrRuleAction> BrRuleActions { get; set; }
    public virtual DbSet<BrRuleSchedule> BrRuleSchedules { get; set; }
    public virtual DbSet<BrRuleExecution> BrRuleExecutions { get; set; }
    public virtual DbSet<BrRuleExecutionLog> BrRuleExecutionLogs { get; set; }
    public virtual DbSet<BrRuleTemplate> BrRuleTemplates { get; set; }

    partial void ConfigureBusinessRulesModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BrRule>(e =>
        {
            e.ToTable("br_rules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.Name).HasMaxLength(120).HasColumnName("name");
            e.Property(x => x.Description).HasMaxLength(500).HasColumnName("description");
            e.Property(x => x.Category).HasMaxLength(40).HasColumnName("category");
            e.Property(x => x.Priority).HasColumnName("priority");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
            e.Property(x => x.CurrentVersionNumber).HasColumnName("current_version_number");
            e.Property(x => x.EffectiveFromUtc).HasColumnName("effective_from_utc");
            e.Property(x => x.EffectiveToUtc).HasColumnName("effective_to_utc");
            e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            e.Property(x => x.RequireApprovalToPublish).HasColumnName("require_approval_to_publish");
            e.Property(x => x.TemplateCode).HasMaxLength(80).HasColumnName("template_code");
            e.HasIndex(x => new { x.CompanyId, x.Status, x.Priority }).HasDatabaseName("IX_br_rules_company_status");
            e.HasMany(x => x.Versions).WithOne(x => x.Rule!).HasForeignKey(x => x.RuleId);
        });

        modelBuilder.Entity<BrRuleVersion>(e =>
        {
            e.ToTable("br_rule_versions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RuleId).HasColumnName("rule_id");
            e.Property(x => x.VersionNumber).HasColumnName("version_number");
            e.Property(x => x.RootLogic).HasConversion<string>().HasMaxLength(10).HasColumnName("root_logic");
            e.Property(x => x.FlowJson).HasColumnName("flow_json");
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.Property(x => x.IsPublished).HasColumnName("is_published");
            e.HasIndex(x => new { x.RuleId, x.VersionNumber }).IsUnique().HasDatabaseName("IX_br_rule_versions_unique");
            e.HasMany(x => x.Conditions).WithOne(x => x.RuleVersion!).HasForeignKey(x => x.RuleVersionId);
            e.HasMany(x => x.Actions).WithOne(x => x.RuleVersion!).HasForeignKey(x => x.RuleVersionId);
        });

        modelBuilder.Entity<BrRuleCondition>(e =>
        {
            e.ToTable("br_rule_conditions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RuleVersionId).HasColumnName("rule_version_id");
            e.Property(x => x.SortOrder).HasColumnName("sort_order");
            e.Property(x => x.Negate).HasColumnName("negate");
            e.Property(x => x.FieldKey).HasMaxLength(80).HasColumnName("field_key");
            e.Property(x => x.Operator).HasConversion<string>().HasMaxLength(20).HasColumnName("operator");
            e.Property(x => x.ValueJson).HasMaxLength(500).HasColumnName("value_json");
        });

        modelBuilder.Entity<BrRuleAction>(e =>
        {
            e.ToTable("br_rule_actions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RuleVersionId).HasColumnName("rule_version_id");
            e.Property(x => x.SortOrder).HasColumnName("sort_order");
            e.Property(x => x.ActionType).HasConversion<string>().HasMaxLength(40).HasColumnName("action_type");
            e.Property(x => x.ParametersJson).HasColumnName("parameters_json");
        });

        modelBuilder.Entity<BrRuleSchedule>(e =>
        {
            e.ToTable("br_rule_schedules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RuleId).HasColumnName("rule_id");
            e.Property(x => x.CronOrInterval).HasMaxLength(40).HasColumnName("cron_or_interval");
            e.Property(x => x.IsEnabled).HasColumnName("is_enabled");
            e.Property(x => x.LastRunUtc).HasColumnName("last_run_utc");
        });

        modelBuilder.Entity<BrRuleExecution>(e =>
        {
            e.ToTable("br_rule_executions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RuleId).HasColumnName("rule_id");
            e.Property(x => x.RuleVersionId).HasColumnName("rule_version_id");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.Mode).HasConversion<string>().HasMaxLength(20).HasColumnName("mode");
            e.Property(x => x.Result).HasConversion<string>().HasMaxLength(20).HasColumnName("result");
            e.Property(x => x.DedupeKey).HasMaxLength(120).HasColumnName("dedupe_key");
            e.Property(x => x.FactsJson).HasColumnName("facts_json");
            e.Property(x => x.ErrorMessage).HasColumnName("error_message");
            e.Property(x => x.DurationMs).HasColumnName("duration_ms");
            e.Property(x => x.TriggeredByUserId).HasColumnName("triggered_by_user_id");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.DedupeKey }).HasDatabaseName("IX_br_exec_dedupe");
            e.HasMany(x => x.Logs).WithOne().HasForeignKey(x => x.ExecutionId);
        });

        modelBuilder.Entity<BrRuleExecutionLog>(e =>
        {
            e.ToTable("br_rule_execution_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.ExecutionId).HasColumnName("execution_id");
            e.Property(x => x.StepType).HasMaxLength(40).HasColumnName("step_type");
            e.Property(x => x.Message).HasMaxLength(200).HasColumnName("message");
            e.Property(x => x.DetailJson).HasColumnName("detail_json");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.Entity<BrRuleTemplate>(e =>
        {
            e.ToTable("br_rule_templates");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Code).HasMaxLength(40).HasColumnName("code");
            e.Property(x => x.Name).HasMaxLength(120).HasColumnName("name");
            e.Property(x => x.Description).HasMaxLength(500).HasColumnName("description");
            e.Property(x => x.Category).HasMaxLength(40).HasColumnName("category");
            e.Property(x => x.FlowJson).HasColumnName("flow_json");
            e.Property(x => x.IsSystem).HasColumnName("is_system");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_br_templates_code");
        });
    }
}
