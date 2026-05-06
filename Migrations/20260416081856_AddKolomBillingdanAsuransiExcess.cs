using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomBillingdanAsuransiExcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SubTotalAsuransiExcess",
                schema: "public",
                table: "MainKasir",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiExcessId",
                schema: "public",
                table: "Billing",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCoveredExcess",
                schema: "public",
                table: "Billing",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubTotalAsuransiExcess",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "AsuransiExcessId",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "IsCoveredExcess",
                schema: "public",
                table: "Billing");
        }
    }
}
