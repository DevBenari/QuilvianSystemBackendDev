using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahNamaKolomTTDDiIGDAssessmentAwal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TTDPerawatId",
                table: "IGDAssessmentAwals",
                newName: "TTDUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TTDUserId",
                table: "IGDAssessmentAwals",
                newName: "TTDPerawatId");
        }
    }
}
