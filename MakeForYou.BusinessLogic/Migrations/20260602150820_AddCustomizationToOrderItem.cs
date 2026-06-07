using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakeForYou.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizationToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCustomization",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomizationNote",
                table: "OrderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomizationResolved",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCustomization",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CustomizationNote",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsCustomizationResolved",
                table: "OrderItems");
        }
    }
}
