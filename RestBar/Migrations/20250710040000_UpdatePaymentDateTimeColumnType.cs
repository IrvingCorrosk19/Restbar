using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentDateTimeColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cambiar el tipo de columna paid_at de timestamp without time zone a timestamp with time zone
            migrationBuilder.Sql("ALTER TABLE payments ALTER COLUMN paid_at TYPE timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir el cambio
            migrationBuilder.Sql("ALTER TABLE payments ALTER COLUMN paid_at TYPE timestamp without time zone");
        }
    }
} 