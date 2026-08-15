using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Cards.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePasswordsFromRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Hash",
                table: "Rooms",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Salt",
                table: "Rooms",
                type: "bytea",
                nullable: true);
        }
    }
}
