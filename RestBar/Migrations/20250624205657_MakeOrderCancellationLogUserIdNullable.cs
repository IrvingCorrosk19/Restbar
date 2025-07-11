using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class MakeOrderCancellationLogUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "order_cancellation_logs_user_id_fkey",
                table: "order_cancellation_logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "order_cancellation_logs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "order_cancellation_logs_user_id_fkey",
                table: "order_cancellation_logs",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "order_cancellation_logs_user_id_fkey",
                table: "order_cancellation_logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "order_cancellation_logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "order_cancellation_logs_user_id_fkey",
                table: "order_cancellation_logs",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
