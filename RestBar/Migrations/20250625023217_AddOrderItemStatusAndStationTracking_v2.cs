using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemStatusAndStationTracking_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PreparedAt",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreparedByStationId",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "order_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_order_items_PreparedByStationId",
                table: "order_items",
                column: "PreparedByStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_stations_PreparedByStationId",
                table: "order_items",
                column: "PreparedByStationId",
                principalTable: "stations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_stations_PreparedByStationId",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_order_items_PreparedByStationId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "PreparedAt",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "PreparedByStationId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "order_items");
        }
    }
}
