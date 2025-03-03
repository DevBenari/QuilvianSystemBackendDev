using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editpasienbaru3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoBase64",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.AddColumn<string>(
                name: "JudulFileFoto",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JudulFileFoto",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.AddColumn<string>(
                name: "FotoBase64",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "TEXT",
                nullable: true);
        }
    }
}
