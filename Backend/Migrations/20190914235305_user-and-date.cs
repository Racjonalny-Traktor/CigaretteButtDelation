using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace microserv.Migrations
{
    public partial class useranddate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Litters",
                nullable: false,
                defaultValueSql: "getdate()");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Litters",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Litters");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Litters");
        }
    }
}
