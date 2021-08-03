using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Silky.Account.Database.Migrations.Migrations
{
    public partial class v100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(type: "varchar(100) CHARACTER SET utf8mb4", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "varchar(100) CHARACTER SET utf8mb4", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "varchar(500) CHARACTER SET utf8mb4", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "varchar(100) CHARACTER SET utf8mb4", maxLength: 100, nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LockBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BalanceRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PayStatus = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceRecords", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Address", "Balance", "CreateBy", "CreateTime", "Email", "LockBalance", "Password", "UpdateBy", "UpdateTime", "UserName" },
                values: new object[] { 1L, "beijing", 200m, null, new DateTime(2021, 8, 3, 21, 35, 12, 371, DateTimeKind.Local).AddTicks(8680), "admin@silky.com", 0m, "de4b550727f5b0ff46328be48c0765c3", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin" });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Address", "Balance", "CreateBy", "CreateTime", "Email", "LockBalance", "Password", "UpdateBy", "UpdateTime", "UserName" },
                values: new object[] { 2L, "beijing", 500m, null, new DateTime(2021, 8, 3, 21, 35, 12, 371, DateTimeKind.Local).AddTicks(8990), "liuhll@silky.com", 0m, "909e74b36a584cb99e9a83636933a39b", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "liuhll" });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Address", "Balance", "CreateBy", "CreateTime", "Email", "LockBalance", "Password", "UpdateBy", "UpdateTime", "UserName" },
                values: new object[] { 3L, "shenzhen", 3000m, null, new DateTime(2021, 8, 3, 21, 35, 12, 371, DateTimeKind.Local).AddTicks(9030), "lisi@silky.com", 0m, "01efe69ed557b5525b5cc4c8f98ac7db", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "lisi" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "BalanceRecords");
        }
    }
}
