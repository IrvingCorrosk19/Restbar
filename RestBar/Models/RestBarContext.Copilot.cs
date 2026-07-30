using Microsoft.EntityFrameworkCore;

namespace RestBar.Models;

public partial class RestBarContext
{
    public virtual DbSet<CopilotConversation> CopilotConversations { get; set; }
    public virtual DbSet<CopilotMessage> CopilotMessages { get; set; }
    public virtual DbSet<CopilotMemoryItem> CopilotMemoryItems { get; set; }
    public virtual DbSet<CopilotAuditEvent> CopilotAuditEvents { get; set; }
    public virtual DbSet<CopilotActionLog> CopilotActionLogs { get; set; }

    partial void ConfigureCopilotModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CopilotConversation>(e =>
        {
            e.ToTable("copilot_conversations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Title).HasMaxLength(120).HasColumnName("title");
            e.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
            e.Property(x => x.LastMessageAtUtc).HasColumnName("last_message_at_utc");
            e.Property(x => x.IsClosed).HasColumnName("is_closed");
            e.HasIndex(x => new { x.CompanyId, x.UserId, x.LastMessageAtUtc }).HasDatabaseName("IX_copilot_conversations_user");
        });

        modelBuilder.Entity<CopilotMessage>(e =>
        {
            e.ToTable("copilot_messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.ConversationId).HasColumnName("conversation_id");
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).HasColumnName("role");
            e.Property(x => x.Intent).HasConversion<string>().HasMaxLength(40).HasColumnName("intent");
            e.Property(x => x.Content).HasColumnName("content");
            e.Property(x => x.ToolsJson).HasColumnName("tools_json");
            e.Property(x => x.DurationMs).HasColumnName("duration_ms");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasOne(x => x.Conversation).WithMany(c => c.Messages).HasForeignKey(x => x.ConversationId);
            e.HasIndex(x => new { x.ConversationId, x.CreatedAtUtc }).HasDatabaseName("IX_copilot_messages_conversation");
        });

        modelBuilder.Entity<CopilotMemoryItem>(e =>
        {
            e.ToTable("copilot_memory_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Key).HasMaxLength(80).HasColumnName("key");
            e.Property(x => x.Value).HasColumnName("value");
            e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.UserId, x.Key }).IsUnique().HasDatabaseName("IX_copilot_memory_unique");
        });

        modelBuilder.Entity<CopilotAuditEvent>(e =>
        {
            e.ToTable("copilot_audit_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ConversationId).HasColumnName("conversation_id");
            e.Property(x => x.Question).HasColumnName("question");
            e.Property(x => x.AnswerDigest).HasColumnName("answer_digest");
            e.Property(x => x.ToolsJson).HasColumnName("tools_json");
            e.Property(x => x.Provider).HasMaxLength(40).HasColumnName("provider");
            e.Property(x => x.Intent).HasMaxLength(40).HasColumnName("intent");
            e.Property(x => x.DurationMs).HasColumnName("duration_ms");
            e.Property(x => x.TokensEst).HasColumnName("tokens_est");
            e.Property(x => x.Success).HasColumnName("success");
            e.Property(x => x.ContentHash).HasMaxLength(64).HasColumnName("content_hash");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            e.HasIndex(x => new { x.CompanyId, x.CreatedAtUtc }).HasDatabaseName("IX_copilot_audit_company");
        });

        modelBuilder.Entity<CopilotActionLog>(e =>
        {
            e.ToTable("copilot_action_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CompanyId).HasColumnName("company_id");
            e.Property(x => x.BranchId).HasColumnName("branch_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ActionCode).HasMaxLength(80).HasColumnName("action_code");
            e.Property(x => x.PayloadJson).HasColumnName("payload_json");
            e.Property(x => x.Succeeded).HasColumnName("succeeded");
            e.Property(x => x.Error).HasMaxLength(500).HasColumnName("error");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        });
    }
}
