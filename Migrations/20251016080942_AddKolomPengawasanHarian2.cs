using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPengawasanHarian2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PainAssesment",
                table: "PengawasanHarians",
                newName: "PainAssesmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PainAssesmentId",
                table: "PengawasanHarians",
                newName: "PainAssesment");
        }
    }
}
