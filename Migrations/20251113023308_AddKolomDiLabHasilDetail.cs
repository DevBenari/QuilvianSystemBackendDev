using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiLabHasilDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PerkiraanPenyakit",
                table: "LabBookingDetails",
                newName: "StatusPemeriksaan");

            migrationBuilder.AddColumn<Guid>(
                name: "AnalisId",
                table: "LabHasilDetails",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "HasilMakroskopik",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilMikroskopik",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefinitif",
                table: "LabHasilDetails",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDuplu",
                table: "LabHasilDetails",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JaringanVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanSpecimen",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KesimpulanHasil",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NilaiNormal",
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

            migrationBuilder.AddColumn<decimal>(
                name: "UrineVolume",
                table: "LabHasilDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoLab",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPA",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusBookingLab",
                table: "LabBookings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusVerifikasi",
                table: "LabBookingDetails",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalSelesai",
                table: "LabBookingDetails",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalisId",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "BloodVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "BodyFluidVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "HasilMakroskopik",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "HasilMikroskopik",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "IsDefinitif",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "IsDuplu",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "JaringanVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "KeadaanSpecimen",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "KesimpulanHasil",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "NilaiNormal",
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
                name: "UrineVolume",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "NoLab",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "NoPA",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "StatusBookingLab",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "StatusVerifikasi",
                table: "LabBookingDetails");

            migrationBuilder.DropColumn(
                name: "TanggalSelesai",
                table: "LabBookingDetails");

            migrationBuilder.RenameColumn(
                name: "StatusPemeriksaan",
                table: "LabBookingDetails",
                newName: "PerkiraanPenyakit");
        }
    }
}
