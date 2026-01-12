using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTabmbahanSuratPengantarRanap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HarapanHasil",
                table: "SuratPengantarRawatInaps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndikasiTindakan",
                table: "SuratPengantarRawatInaps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdaHambatan",
                table: "SuratPengantarRawatInaps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisOperasi",
                table: "SuratPengantarRawatInaps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathTTDDokterDPJP",
                table: "SuratPengantarRawatInaps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TawaranLayanan",
                table: "SuratPengantarRawatInaps",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HarapanHasil",
                table: "SuratPengantarRawatInaps");

            migrationBuilder.DropColumn(
                name: "IndikasiTindakan",
                table: "SuratPengantarRawatInaps");

            migrationBuilder.DropColumn(
                name: "IsAdaHambatan",
                table: "SuratPengantarRawatInaps");

            migrationBuilder.DropColumn(
                name: "JenisOperasi",
                table: "SuratPengantarRawatInaps");

            migrationBuilder.DropColumn(
                name: "PathTTDDokterDPJP",
                table: "SuratPengantarRawatInaps");

            migrationBuilder.DropColumn(
                name: "TawaranLayanan",
                table: "SuratPengantarRawatInaps");
        }
    }
}
