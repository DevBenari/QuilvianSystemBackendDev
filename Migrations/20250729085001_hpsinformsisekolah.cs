using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hpsinformsisekolah : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InformasiSekolah",
                schema: "public",
                table: "PdfPasienBaru");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InformasiSekolah",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);
        }
    }
}
