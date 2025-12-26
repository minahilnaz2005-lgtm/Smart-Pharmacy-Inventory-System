using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPIEMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixMedicineBatchSupplierId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "MedicineBatches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_SupplierId",
                table: "MedicineBatches",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineBatches_Suppliers_SupplierId",
                table: "MedicineBatches",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicineBatches_Suppliers_SupplierId",
                table: "MedicineBatches");

            migrationBuilder.DropIndex(
                name: "IX_MedicineBatches_SupplierId",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "MedicineBatches");
        }
    }
}
