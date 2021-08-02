using Microsoft.EntityFrameworkCore.Migrations;

namespace Silky.Account.Database.Migrations.Migrations
{
    public partial class v101 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Accounts",
                newName: "UserName");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Accounts",
                type: "varchar(20) CHARACTER SET utf8mb4",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Accounts",
                newName: "UserName");
        }
    }
}
