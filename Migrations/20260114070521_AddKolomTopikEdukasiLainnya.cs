using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTopikEdukasiLainnya : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EdukasiDetail",
                table: "AssesmentEdukasiDetails",
                newName: "TopikEdukasiLainnya");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TopikEdukasiLainnya",
                table: "AssesmentEdukasiDetails",
                newName: "EdukasiDetail");
        }
    }
}
