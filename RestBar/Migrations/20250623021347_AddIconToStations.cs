﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class AddIconToStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "stations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "stations");
        }
    }
}
