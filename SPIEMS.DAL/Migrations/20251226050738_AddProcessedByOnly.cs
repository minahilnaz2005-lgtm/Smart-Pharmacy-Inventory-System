using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPIEMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedByOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessedBy",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedBy",
                table: "Sales");
        }
    }
}
