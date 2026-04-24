using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditKolomLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailIcdId",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "LabId",
                table: "LabBookings");

            migrationBuilder.AddColumn<string>(
                name: "DiagnosaAwal",
                table: "LabBookings",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiagnosaAwal",
                table: "LabBookings");

            migrationBuilder.AddColumn<Guid>(
                name: "DetailIcdId",
                table: "LabBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LabId",
                table: "LabBookings",
                type: "uuid",
                nullable: true);
        }
    }
}
