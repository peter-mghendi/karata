using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Cards.Data.Migrations;

/// <inheritdoc />
public partial class _20261123214805_MoveTurnProblemToTurn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Metadata",
            table: "Turns");

        migrationBuilder.AddColumn<string>(
            name: "Problem",
            table: "Turns",
            type: "jsonb",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Problem",
            table: "Turns");

        migrationBuilder.AddColumn<string>(
            name: "Metadata",
            table: "Turns",
            type: "text",
            nullable: false,
            defaultValue: "");
    }
}
