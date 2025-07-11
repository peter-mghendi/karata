﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToHand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Hands",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Hands");
        }
    }
}
