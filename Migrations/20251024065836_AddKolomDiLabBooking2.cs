using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiLabBooking2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiId",
                table: "LabBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPemeriksaan",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglPemeriksaan",
                table: "LabBookings",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsuransiId",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "StatusPemeriksaan",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "TglPemeriksaan",
                table: "LabBookings");
        }
    }
}
