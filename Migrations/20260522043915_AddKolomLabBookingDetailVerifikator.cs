using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomLabBookingDetailVerifikator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VerifikatorId",
                schema: "public",
                table: "LabBookingDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaktuVerifikasi",
                schema: "public",
                table: "LabBookingDetail",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifikatorId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "WaktuVerifikasi",
                schema: "public",
                table: "LabBookingDetail");
        }
    }
}
