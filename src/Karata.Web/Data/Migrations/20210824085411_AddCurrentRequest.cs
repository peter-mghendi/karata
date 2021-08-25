using Microsoft.EntityFrameworkCore.Migrations;

namespace Karata.Web.Data.Migrations
{
    public partial class AddCurrentRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Started",
                table: "Games",
                newName: "Pick");

            migrationBuilder.AddColumn<string>(
                name: "CurrentRequest",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForward",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarted",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRequest",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsForward",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsStarted",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "Pick",
                table: "Games",
                newName: "Started");
        }
    }
}
