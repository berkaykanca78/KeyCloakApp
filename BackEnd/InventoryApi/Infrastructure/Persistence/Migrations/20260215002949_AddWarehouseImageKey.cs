using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseImageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageKey",
                table: "Warehouses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageKey",
                table: "Warehouses");
        }
    }
}
