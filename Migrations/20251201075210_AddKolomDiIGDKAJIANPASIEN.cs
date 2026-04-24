using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiIGDKAJIANPASIEN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BahasaDigunakan",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNAbdomen",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNEkstremitas",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNGenital",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNJantung",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNKepala",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNLeher",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNMata",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNMulut",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNParu",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNPunggung",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNTHT",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNThorak",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisHambatan",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanEkstremitas",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanKepala",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanLeher",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanMata",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanMulut",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanPunggung",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanTHT",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanThorak",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bangsal",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndikasiRanap",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisum",
                table: "IGDTindakLanjuts",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JumlahHariIzin",
                table: "IGDTindakLanjuts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanPasienPulang",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KelasId",
                table: "IGDTindakLanjuts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KesimpulanAkhir",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilisasiSaatPulang",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observasi",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PenyebabMeninggal",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TTDDokterId",
                table: "IGDTindakLanjuts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TTDPerawatId",
                table: "IGDTindakLanjuts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalAkhirIzin",
                table: "IGDTindakLanjuts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalAwalIzin",
                table: "IGDTindakLanjuts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalMeninggal",
                table: "IGDTindakLanjuts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TempatMeninggal",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TindakLanjut",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UPF",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WaktuDipulangkan",
                table: "IGDTindakLanjuts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WaktuDirujuk",
                table: "IGDTindakLanjuts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterId",
                table: "IGDTindakanDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilMedikamentosa",
                table: "IGDTindakanDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilSkinTest",
                table: "IGDTindakanDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilTetanusToxoid",
                table: "IGDTindakanDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JalurMedikamentosa",
                table: "IGDTindakanDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JumlahAntiTetanusSerum",
                table: "IGDTindakanDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerawatId",
                table: "IGDTindakanDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WaktuPengobatan",
                table: "IGDTindakanDetails",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LokasiTrauma",
                table: "IGDPasienDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalTrauma",
                table: "IGDPasienDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ATS",
                table: "IGDObservasis",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosa",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilAlloanamnesis",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HasilLabId",
                table: "IGDAssessmentAwals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnamnesis",
                table: "IGDAssessmentAwals",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KondisiUmum",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathGambarPenandaan",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pemeriksaan",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TanggalPencatatan",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BahasaDigunakan",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNAbdomen",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNEkstremitas",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNGenital",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNJantung",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNKepala",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNLeher",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNMata",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNMulut",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNParu",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNPunggung",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNTHT",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDBNThorak",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "JenisHambatan",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanEkstremitas",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanKepala",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanLeher",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanMata",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanMulut",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanPunggung",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanTHT",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KeadaanThorak",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "Bangsal",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "IndikasiRanap",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "IsVisum",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "JumlahHariIzin",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "KeadaanPasienPulang",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "KelasId",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "KesimpulanAkhir",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "MobilisasiSaatPulang",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "Observasi",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "PenyebabMeninggal",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TTDDokterId",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TTDPerawatId",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TanggalAkhirIzin",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TanggalAwalIzin",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TanggalMeninggal",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TempatMeninggal",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TindakLanjut",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "UPF",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "WaktuDipulangkan",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "WaktuDirujuk",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "DokterId",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "HasilMedikamentosa",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "HasilSkinTest",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "HasilTetanusToxoid",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "JalurMedikamentosa",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "JumlahAntiTetanusSerum",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "PerawatId",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "WaktuPengobatan",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "LokasiTrauma",
                table: "IGDPasienDetails");

            migrationBuilder.DropColumn(
                name: "TanggalTrauma",
                table: "IGDPasienDetails");

            migrationBuilder.DropColumn(
                name: "ATS",
                table: "IGDObservasis");

            migrationBuilder.DropColumn(
                name: "Diagnosa",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "HasilAlloanamnesis",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "HasilLabId",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "IsAnamnesis",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "KondisiUmum",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "PathGambarPenandaan",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "Pemeriksaan",
                table: "IGDAssessmentAwals");

            migrationBuilder.DropColumn(
                name: "TanggalPencatatan",
                table: "IGDAssessmentAwals");
        }
    }
}
