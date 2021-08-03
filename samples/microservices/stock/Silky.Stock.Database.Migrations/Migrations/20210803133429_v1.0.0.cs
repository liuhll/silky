using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Silky.Stock.Database.Migrations.Migrations
{
    public partial class v100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100) CHARACTER SET utf8mb4", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    LockStock = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreateBy", "CreateTime", "LockStock", "Name", "Stock", "UnitPrice", "UpdateBy", "UpdateTime" },
                values: new object[] { 1L, null, new DateTime(2021, 8, 3, 21, 34, 29, 499, DateTimeKind.Local).AddTicks(6130), 0, "iPhone11", 100, 10m, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreateBy", "CreateTime", "LockStock", "Name", "Stock", "UnitPrice", "UpdateBy", "UpdateTime" },
                values: new object[] { 2L, null, new DateTime(2021, 8, 3, 21, 34, 29, 499, DateTimeKind.Local).AddTicks(6170), 0, "huawei", 200, 120m, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreateBy", "CreateTime", "LockStock", "Name", "Stock", "UnitPrice", "UpdateBy", "UpdateTime" },
                values: new object[] { 3L, null, new DateTime(2021, 8, 3, 21, 34, 29, 499, DateTimeKind.Local).AddTicks(6170), 0, "xiaomi", 150, 50m, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
