using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakeForYou.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class PayChannelIntergration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Sellers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Sellers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBin",
                table: "Sellers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSellerPaid",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PayoutReferenceId",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "BankBin",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "IsSellerPaid",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PayoutReferenceId",
                table: "Orders");
        }
    }
}
