using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakeForYou.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerShopFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressDetail",
                table: "Sellers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupFullName",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupPhone",
                table: "Sellers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopDescription",
                table: "Sellers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "Sellers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressDetail",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "PickupFullName",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "PickupPhone",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "ShopDescription",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "ShopName",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Sellers");
        }
    }
}
