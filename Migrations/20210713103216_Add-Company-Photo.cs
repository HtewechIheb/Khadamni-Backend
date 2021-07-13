using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Project_X.Migrations
{
    public partial class AddCompanyPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PhotoFile",
                table: "Companies",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "PhotoFileName",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoFile",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PhotoFileName",
                table: "Companies");
        }
    }
}
