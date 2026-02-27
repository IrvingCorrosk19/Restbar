using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStationIdFromProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "products_station_id_fkey",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_station_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "station_id",
                table: "products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "station_id",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_station_id",
                table: "products",
                column: "station_id");

            migrationBuilder.AddForeignKey(
                name: "products_station_id_fkey",
                table: "products",
                column: "station_id",
                principalTable: "stations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
