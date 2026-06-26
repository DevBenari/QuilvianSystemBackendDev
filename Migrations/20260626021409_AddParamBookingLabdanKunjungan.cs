using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamBookingLabdanKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KunjunganLab",
                schema: "public",
                table: "MstKunjungan",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiskonId",
                schema: "public",
                table: "LabBooking",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuratRujukan",
                schema: "public",
                table: "LabBooking",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_DiskonId",
                schema: "public",
                table: "LabBooking",
                column: "DiskonId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_Diskon_DiskonId",
                schema: "public",
                table: "LabBooking",
                column: "DiskonId",
                principalSchema: "public",
                principalTable: "Diskon",
                principalColumn: "DiskonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_Diskon_DiskonId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_DiskonId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "KunjunganLab",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "DiskonId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "SuratRujukan",
                schema: "public",
                table: "LabBooking");
        }
    }
}
