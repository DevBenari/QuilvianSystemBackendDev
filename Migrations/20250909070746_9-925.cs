using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class _9925 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Berat",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KesehatanUmum",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KondisiFisik",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TesNarkoba",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tinggi",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Berat",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "KesehatanUmum",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "KondisiFisik",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "TesNarkoba",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "Tinggi",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");
        }
    }
}
