using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataToTurn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Turns",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Turns");
        }
    }
}
