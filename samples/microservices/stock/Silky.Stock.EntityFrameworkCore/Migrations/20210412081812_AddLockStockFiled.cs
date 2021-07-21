using Microsoft.EntityFrameworkCore.Migrations;

namespace Silky.Stock.EntityFrameworkCore.Migrations
{
    public partial class AddLockStockFiled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LockStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockStock",
                table: "Products");
        }
    }
}
