using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class AddSentAtToOrderItemOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<int>(
            //     name: "KitchenStatus",
            //     table: "order_items",
            //     type: "integer",
            //     nullable: false,
            //     defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "KitchenStatus",
            //     table: "order_items");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "order_items");
        }
    }
}
