using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTipeLayanan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RanapId",
                table: "TindakanKunjungans");

            migrationBuilder.AddColumn<string>(
                name: "TipeLayanan",
                table: "TindakanKunjungans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeLayanan",
                table: "LabBookingDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeLayanan",
                schema: "public",
                table: "Billing",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipeLayanan",
                table: "TindakanKunjungans");

            migrationBuilder.DropColumn(
                name: "TipeLayanan",
                table: "LabBookingDetails");

            migrationBuilder.DropColumn(
                name: "TipeLayanan",
                schema: "public",
                table: "Billing");

            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                table: "TindakanKunjungans",
                type: "uuid",
                nullable: true);
        }
    }
}
