using Microsoft.EntityFrameworkCore.Migrations;

namespace Project_X.Migrations
{
    public partial class AddCandidateResumeFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Resume",
                table: "Candidates",
                newName: "ResumeFile");

            migrationBuilder.AddColumn<string>(
                name: "ResumeFileName",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeFileName",
                table: "Candidates");

            migrationBuilder.RenameColumn(
                name: "ResumeFile",
                table: "Candidates",
                newName: "Resume");
        }
    }
}
