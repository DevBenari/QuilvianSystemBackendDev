using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomLabBookingDanLabHasil : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JumlahKirimHasil",
                table: "LabHasils",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusKirimHasil",
                table: "LabHasils",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TanggalKirimHasilTerakhir",
                table: "LabHasils",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusTercover",
                schema: "public",
                table: "LabBookingDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KalkulasiTercover",
                schema: "public",
                table: "LabBooking",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KalkulasiTidakTercover",
                schema: "public",
                table: "LabBooking",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JumlahKirimHasil",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "StatusKirimHasil",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "TanggalKirimHasilTerakhir",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "StatusTercover",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "KalkulasiTercover",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "KalkulasiTidakTercover",
                schema: "public",
                table: "LabBooking");
        }
    }
}
