using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiLabBooking3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SuratJaminanId",
                table: "LabBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuratJaminanPath",
                table: "LabBookings",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuratJaminanId",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "SuratJaminanPath",
                table: "LabBookings");
        }
    }
}
