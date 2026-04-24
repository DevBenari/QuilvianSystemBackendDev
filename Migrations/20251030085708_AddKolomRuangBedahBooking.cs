using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomRuangBedahBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTerverifikasi",
                table: "RuangBedahBookings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglSelesai",
                table: "RuangBedahBookings",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTerverifikasi",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "TglSelesai",
                table: "RuangBedahBookings");
        }
    }
}
