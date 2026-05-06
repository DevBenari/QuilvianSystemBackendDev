using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class HapusKolomDiKonversiSatuan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaSatuan",
                schema: "public",
                table: "MstKonversiSatuan");

            migrationBuilder.DropColumn(
                name: "TipeKonversi",
                schema: "public",
                table: "MstKonversiSatuan");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamaSatuan",
                schema: "public",
                table: "MstKonversiSatuan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeKonversi",
                schema: "public",
                table: "MstKonversiSatuan",
                type: "text",
                nullable: true);
        }
    }
}
