using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Trivia.Data.Migrations;

/// <inheritdoc />
public partial class _20261125171124_MakeResponseChoiceOptional : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<long>(
            name: "ChoiceId",
            table: "Responses",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<long>(
            name: "ChoiceId",
            table: "Responses",
            type: "bigint",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);
    }
}
