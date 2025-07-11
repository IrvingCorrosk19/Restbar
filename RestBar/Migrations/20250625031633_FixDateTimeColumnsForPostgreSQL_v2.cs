using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class FixDateTimeColumnsForPostgreSQL_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure all DateTime columns are properly configured as timestamp with time zone
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN opened_at TYPE timestamp with time zone USING opened_at AT TIME ZONE 'UTC';");
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN closed_at TYPE timestamp with time zone USING closed_at AT TIME ZONE 'UTC';");
            migrationBuilder.Sql("ALTER TABLE order_items ALTER COLUMN \"PreparedAt\" TYPE timestamp with time zone USING \"PreparedAt\" AT TIME ZONE 'UTC';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to timestamp without time zone if needed
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN opened_at TYPE timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN closed_at TYPE timestamp without time zone;");
            migrationBuilder.Sql("ALTER TABLE order_items ALTER COLUMN \"PreparedAt\" TYPE timestamp without time zone;");
        }
    }
}
