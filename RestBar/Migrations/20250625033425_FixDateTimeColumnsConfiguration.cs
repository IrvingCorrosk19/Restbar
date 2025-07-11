using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class FixDateTimeColumnsConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_stations_PreparedByStationId",
                table: "order_items");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "order_items",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "PreparedByStationId",
                table: "order_items",
                newName: "prepared_by_station_id");

            migrationBuilder.RenameColumn(
                name: "PreparedAt",
                table: "order_items",
                newName: "prepared_at");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_PreparedByStationId",
                table: "order_items",
                newName: "IX_order_items_prepared_by_station_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "opened_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "closed_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "order_items",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "StationId",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_items_StationId",
                table: "order_items",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_stations_StationId",
                table: "order_items",
                column: "StationId",
                principalTable: "stations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "order_items_prepared_by_station_id_fkey",
                table: "order_items",
                column: "prepared_by_station_id",
                principalTable: "stations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_stations_StationId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "order_items_prepared_by_station_id_fkey",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_order_items_StationId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "order_items");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "order_items",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prepared_by_station_id",
                table: "order_items",
                newName: "PreparedByStationId");

            migrationBuilder.RenameColumn(
                name: "prepared_at",
                table: "order_items",
                newName: "PreparedAt");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_prepared_by_station_id",
                table: "order_items",
                newName: "IX_order_items_PreparedByStationId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "opened_at",
                table: "orders",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "closed_at",
                table: "orders",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "order_items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_stations_PreparedByStationId",
                table: "order_items",
                column: "PreparedByStationId",
                principalTable: "stations",
                principalColumn: "id");
        }
    }
}
