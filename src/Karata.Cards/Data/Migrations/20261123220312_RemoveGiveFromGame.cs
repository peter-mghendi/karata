using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Cards.Data.Migrations;

/// <inheritdoc />
public partial class _20261123220312_RemoveGiveFromGame : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Give",
            table: "Games");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "Give",
            table: "Games",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);
    }
}
