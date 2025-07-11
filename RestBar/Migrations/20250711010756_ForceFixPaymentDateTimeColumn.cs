using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class ForceFixPaymentDateTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Forzar el cambio de tipo de columna paid_at a timestamp with time zone
            migrationBuilder.Sql(@"
                ALTER TABLE payments 
                ALTER COLUMN paid_at TYPE timestamp with time zone 
                USING paid_at AT TIME ZONE 'UTC';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir el cambio si es necesario
            migrationBuilder.Sql(@"
                ALTER TABLE payments 
                ALTER COLUMN paid_at TYPE timestamp without time zone;
            ");
        }
    }
}
