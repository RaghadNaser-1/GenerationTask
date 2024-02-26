using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenerationTask.Migrations
{
    /// <inheritdoc />
    public partial class ForthCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "GeneratedPdfs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "GeneratedPdfs");
        }
    }
}
