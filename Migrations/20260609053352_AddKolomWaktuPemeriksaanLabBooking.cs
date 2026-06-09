using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomWaktuPemeriksaanLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsPasienPersiapan",
                schema: "public",
                table: "LabBooking",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WaktuPemeriksaan",
                schema: "public",
                table: "LabBooking",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WaktuPemeriksaanPersiapan",
                schema: "public",
                table: "LabBooking",
                type: "time without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaktuPemeriksaan",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "WaktuPemeriksaanPersiapan",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.AlterColumn<string>(
                name: "IsPasienPersiapan",
                schema: "public",
                table: "LabBooking",
                type: "text",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }
    }
}
