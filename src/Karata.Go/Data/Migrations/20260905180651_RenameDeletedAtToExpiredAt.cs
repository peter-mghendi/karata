using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Go.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeletedAtToExpiredAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Links",
                newName: "ExpiredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiredAt",
                table: "Links",
                newName: "DeletedAt");
        }
    }
}
