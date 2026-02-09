using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTTgInvoiceDiBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DPD",
                schema: "public",
                table: "Billing",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalInvoice",
                schema: "public",
                table: "Billing",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalJatuhTempo",
                schema: "public",
                table: "Billing",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DPD",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "TanggalInvoice",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "TanggalJatuhTempo",
                schema: "public",
                table: "Billing");
        }
    }
}
