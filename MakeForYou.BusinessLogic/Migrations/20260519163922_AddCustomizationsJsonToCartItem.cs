using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakeForYou.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizationsJsonToCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomizationsJson",
                table: "CartItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomizationsJson",
                table: "CartItems");
        }
    }
}
