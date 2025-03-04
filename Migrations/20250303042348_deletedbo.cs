using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class deletedbo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PdfPasienBaru",
                schema: "dbo",
                newName: "PdfPasienBaru",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PdfPasien",
                schema: "dbo",
                newName: "PdfPasien",
                newSchema: "public");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "PdfPasienBaru",
                schema: "public",
                newName: "PdfPasienBaru",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PdfPasien",
                schema: "public",
                newName: "PdfPasien",
                newSchema: "dbo");
        }
    }
}
