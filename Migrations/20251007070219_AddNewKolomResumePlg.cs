using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewKolomResumePlg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KunjunganId",
                table: "ResumePulangDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PasienID",
                table: "ResumePulangDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PemakaianWC",
                table: "ResumePulangDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosa",
                table: "LabBookingDetails",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KunjunganId",
                table: "ResumePulangDetails");

            migrationBuilder.DropColumn(
                name: "PasienID",
                table: "ResumePulangDetails");

            migrationBuilder.DropColumn(
                name: "PemakaianWC",
                table: "ResumePulangDetails");

            migrationBuilder.DropColumn(
                name: "Diagnosa",
                table: "LabBookingDetails");
        }
    }
}
