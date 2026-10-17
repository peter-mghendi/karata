using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karata.Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimescale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:timescaledb", ",,");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:timescaledb", ",,");
        }
    }
}
