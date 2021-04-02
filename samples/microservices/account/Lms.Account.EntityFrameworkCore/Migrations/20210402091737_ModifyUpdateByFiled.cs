using Microsoft.EntityFrameworkCore.Migrations;

namespace Lms.Account.EntityFrameworkCore.Migrations
{
    public partial class ModifyUpdateByFiled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UpdateBy",
                table: "Accounts",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UpdateBy",
                table: "Accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}
