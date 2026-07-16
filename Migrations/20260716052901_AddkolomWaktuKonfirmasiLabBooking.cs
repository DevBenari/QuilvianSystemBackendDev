using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddkolomWaktuKonfirmasiLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "WaktuKonfirmasi",
                schema: "public",
                table: "LabBooking",
                type: "time without time zone",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "WaktuKonfirmasi",
                schema: "public",
                table: "LabBooking");

        }
    }
}
