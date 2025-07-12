using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderCancellationLogDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Method",
                table: "split_payments",
                newName: "method");

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "split_payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_shared",
                table: "payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payer_name",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Cambiar el tipo de columna Date de timestamp without time zone a timestamp with time zone
            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "order_cancellation_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: false,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_shared",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payer_name",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "method",
                table: "split_payments",
                newName: "Method");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "split_payments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            // Revertir el cambio de tipo de columna
            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "order_cancellation_logs",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: false,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
