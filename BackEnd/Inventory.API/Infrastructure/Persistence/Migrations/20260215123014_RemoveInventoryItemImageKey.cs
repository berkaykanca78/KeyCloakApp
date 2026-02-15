using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInventoryItemImageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageKey",
                table: "InventoryItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageKey",
                table: "InventoryItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
