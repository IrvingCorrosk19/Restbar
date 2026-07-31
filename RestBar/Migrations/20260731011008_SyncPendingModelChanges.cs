using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <summary>
    /// Snapshot sync only. Avoids altering live cash_sessions.row_version concurrency token.
    /// Analytics SQL lives in prior NativeBiAnalyticsLayer / AnalyticsEnterpriseSchema migrations.
    /// </summary>
    public partial class SyncPendingModelChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: model snapshot alignment only.
            // Do not AlterColumn row_version on production cash_sessions.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
