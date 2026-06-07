using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakeForYou.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomizationGroups",
                columns: table => new
                {
                    CustomizationGroupId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationGroups", x => x.CustomizationGroupId);
                    table.ForeignKey(
                        name: "FK_CustomizationGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomizationOptions",
                columns: table => new
                {
                    CustomizationOptionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomizationGroupId = table.Column<long>(type: "bigint", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationOptions", x => x.CustomizationOptionId);
                    table.ForeignKey(
                        name: "FK_CustomizationOptions_CustomizationGroups_CustomizationGroupId",
                        column: x => x.CustomizationGroupId,
                        principalTable: "CustomizationGroups",
                        principalColumn: "CustomizationGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationGroups_ProductId",
                table: "CustomizationGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationOptions_CustomizationGroupId",
                table: "CustomizationOptions",
                column: "CustomizationGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomizationOptions");

            migrationBuilder.DropTable(
                name: "CustomizationGroups");
        }
    }
}
