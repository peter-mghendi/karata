using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Cards.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibilityToRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Rooms",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Rooms");
        }
    }
}
