using Microsoft.EntityFrameworkCore.Migrations;

namespace Karata.Web.Data.Migrations
{
    public partial class AddGiveColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "Give",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Give",
                table: "Games");
        }
    }
}
