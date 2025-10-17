using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hubungankeluarga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HubunganPasien",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "HubunganKeluarga3");

            migrationBuilder.RenameColumn(
                name: "HubunganAnak",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "HubunganKeluarga2");

            migrationBuilder.AddColumn<string>(
                name: "HubunganKeluarga1",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HubunganKeluarga1",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.RenameColumn(
                name: "HubunganKeluarga3",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "HubunganPasien");

            migrationBuilder.RenameColumn(
                name: "HubunganKeluarga2",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "HubunganAnak");
        }
    }
}
