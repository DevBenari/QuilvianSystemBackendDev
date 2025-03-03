using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editDokterPoli4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamaPoliKlinik",
                table: "DokterPolis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaSubPoli",
                table: "DokterPolis",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaPoliKlinik",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "NamaSubPoli",
                table: "DokterPolis");
        }
    }
}
