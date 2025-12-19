using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class TrackAdditionalTurnData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Picked",
                table: "Turns",
                newName: "CardsPlayed");

            migrationBuilder.RenameColumn(
                name: "Cards",
                table: "Turns",
                newName: "CardsPicked");

            migrationBuilder.AddColumn<bool>(
                name: "DeckExhausted",
                table: "Turns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GameResult",
                table: "Turns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameSnapshot",
                table: "Turns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCardless",
                table: "Turns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReclaimedPile",
                table: "Turns",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeckExhausted",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "GameResult",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "GameSnapshot",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "IsCardless",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "ReclaimedPile",
                table: "Turns");

            migrationBuilder.RenameColumn(
                name: "CardsPlayed",
                table: "Turns",
                newName: "Picked");

            migrationBuilder.RenameColumn(
                name: "CardsPicked",
                table: "Turns",
                newName: "Cards");
        }
    }
}
