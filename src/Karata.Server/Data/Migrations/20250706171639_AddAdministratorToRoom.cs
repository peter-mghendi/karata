using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministratorToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdministratorId",
                table: "Rooms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_AdministratorId",
                table: "Rooms",
                column: "AdministratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_AspNetUsers_AdministratorId",
                table: "Rooms",
                column: "AdministratorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_AspNetUsers_AdministratorId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_AdministratorId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "AdministratorId",
                table: "Rooms");
        }
    }
}
