using Microsoft.EntityFrameworkCore.Migrations;

namespace Project_X.Migrations
{
    public partial class AddRelationsBetweenEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "lastName",
                table: "Candidates",
                newName: "LastName");

            migrationBuilder.AddColumn<long>(
                name: "CompanyId",
                table: "Offers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CompanyId",
                table: "Offers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Companies_CompanyId",
                table: "Offers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Companies_CompanyId",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Offers_CompanyId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Offers");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Candidates",
                newName: "lastName");

            migrationBuilder.AddColumn<long>(
               name: "Id",
               table: "Applications",
               type: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                column: "Id");
        }
    }
}
