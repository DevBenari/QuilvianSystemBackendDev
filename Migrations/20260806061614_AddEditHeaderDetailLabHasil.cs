using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddEditHeaderDetailLabHasil : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "BodyFluidVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "JamSpecimen",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "JaringanVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "PetugasSpecimenId",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "PusVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "SputumVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "StoolVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "TanggalSpecimen",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "UrineVolume",
                table: "LabHasilDetails");

            migrationBuilder.AddColumn<string>(
                name: "BahanNonGinekologi",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BloodVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BodyFluidVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosaKlinis",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiksasiDigunakan",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "JamSpecimen",
                table: "LabHasils",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JaringanVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisPemeriksaan",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisSpecimen",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KategoriPemeriksaanPA",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeteranganKlinis",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LokasiSpecimen",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MasaHaidTerakhir",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PetugasSpecimenId",
                table: "LabHasils",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolaTujuanPengambilan",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PusVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiwayatPenyakit",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SputumVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StoolVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalSpecimen",
                table: "LabHasils",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UrineVolume",
                table: "LabHasils",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailDiagnosaKlinis",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HER",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HERImunohistokimia",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ki67",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LainLain",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReseptorEstrogenER",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReseptorProgesteronPR",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusER",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPR",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BahanNonGinekologi",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "BloodVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "BodyFluidVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "DiagnosaKlinis",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "FiksasiDigunakan",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "JamSpecimen",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "JaringanVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "JenisPemeriksaan",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "JenisSpecimen",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "KategoriPemeriksaanPA",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "KeteranganKlinis",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "LokasiSpecimen",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "MasaHaidTerakhir",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "PetugasSpecimenId",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "PolaTujuanPengambilan",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "PusVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "RiwayatPenyakit",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "SputumVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "StoolVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "TanggalSpecimen",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "UrineVolume",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "DetailDiagnosaKlinis",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "HER",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "HERImunohistokimia",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "Ki67",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "LainLain",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "ReseptorEstrogenER",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "ReseptorProgesteronPR",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "StatusER",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "StatusPR",
                table: "LabHasilDetails");

            migrationBuilder.AddColumn<decimal>(
                name: "BloodVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BodyFluidVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "JamSpecimen",
                table: "LabHasilDetails",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JaringanVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PetugasSpecimenId",
                table: "LabHasilDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PusVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SputumVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StoolVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalSpecimen",
                table: "LabHasilDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UrineVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);
        }
    }
}
