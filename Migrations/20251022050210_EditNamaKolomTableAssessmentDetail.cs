using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditNamaKolomTableAssessmentDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TopikEdukasi",
                table: "AssesmentEdukasiDetails",
                newName: "TopikEdukasiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TopikEdukasiId",
                table: "AssesmentEdukasiDetails",
                newName: "TopikEdukasi");
        }
    }
}
