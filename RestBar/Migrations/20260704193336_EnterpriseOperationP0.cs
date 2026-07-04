using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <summary>
    /// P0 Enterprise Operation: propinas, atribución multi-mesero, auditoría de cobros.
    /// </summary>
    public partial class EnterpriseOperationP0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "tip_amount",
                table: "payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "processed_by_user_id",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "added_by_user_id",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "delivered_by_user_id",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_processed_by_user_id",
                table: "payments",
                column: "processed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_added_by_user_id",
                table: "order_items",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_delivered_by_user_id",
                table: "order_items",
                column: "delivered_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "payments_processed_by_user_id_fkey",
                table: "payments",
                column: "processed_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "order_items_added_by_user_id_fkey",
                table: "order_items",
                column: "added_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "order_items_delivered_by_user_id_fkey",
                table: "order_items",
                column: "delivered_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(@"
                UPDATE order_items oi
                SET added_by_user_id = o.user_id
                FROM orders o
                WHERE oi.order_id = o.id
                  AND oi.added_by_user_id IS NULL
                  AND o.user_id IS NOT NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "payments_processed_by_user_id_fkey", table: "payments");
            migrationBuilder.DropForeignKey(name: "order_items_added_by_user_id_fkey", table: "order_items");
            migrationBuilder.DropForeignKey(name: "order_items_delivered_by_user_id_fkey", table: "order_items");

            migrationBuilder.DropIndex(name: "IX_payments_processed_by_user_id", table: "payments");
            migrationBuilder.DropIndex(name: "IX_order_items_added_by_user_id", table: "order_items");
            migrationBuilder.DropIndex(name: "IX_order_items_delivered_by_user_id", table: "order_items");

            migrationBuilder.DropColumn(name: "tip_amount", table: "payments");
            migrationBuilder.DropColumn(name: "processed_by_user_id", table: "payments");
            migrationBuilder.DropColumn(name: "added_by_user_id", table: "order_items");
            migrationBuilder.DropColumn(name: "delivered_by_user_id", table: "order_items");
        }
    }
}
