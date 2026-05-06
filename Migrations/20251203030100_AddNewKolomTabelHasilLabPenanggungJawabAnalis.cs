using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewKolomTabelHasilLabPenanggungJawabAnalis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PenanggungJawabAnalisId",
                table: "LabHasils",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PenanggungJawabId",
                table: "LabHasils",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalPemeriksaan",
                table: "LabHasils",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfoNReff",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "JamSpecimen",
                table: "LabHasilDetails",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalSpecimen",
                table: "LabHasilDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Satuan",
                table: "LabBookingDetails",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenanggungJawabAnalisId",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "PenanggungJawabId",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "TanggalPemeriksaan",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "InfoNReff",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "JamSpecimen",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "TanggalSpecimen",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "Satuan",
                table: "LabBookingDetails");
        }
    }
}
