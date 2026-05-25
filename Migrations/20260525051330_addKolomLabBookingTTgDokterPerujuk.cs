using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addKolomLabBookingTTgDokterPerujuk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DokterPerujukId",
                schema: "public",
                table: "LabBooking",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KonfirmatorId",
                schema: "public",
                table: "LabBooking",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LabId",
                schema: "public",
                table: "LabBooking",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglKonfirmasi",
                schema: "public",
                table: "LabBooking",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_DokterPerujukId",
                schema: "public",
                table: "LabBooking",
                column: "DokterPerujukId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_KonfirmatorId",
                schema: "public",
                table: "LabBooking",
                column: "KonfirmatorId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_LabId",
                schema: "public",
                table: "LabBooking",
                column: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstDokter_DokterPerujukId",
                schema: "public",
                table: "LabBooking",
                column: "DokterPerujukId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstLab_LabId",
                schema: "public",
                table: "LabBooking",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstUserActive_KonfirmatorId",
                schema: "public",
                table: "LabBooking",
                column: "KonfirmatorId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstDokter_DokterPerujukId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstLab_LabId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstUserActive_KonfirmatorId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_DokterPerujukId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_KonfirmatorId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_LabId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "DokterPerujukId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "KonfirmatorId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "LabId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "TglKonfirmasi",
                schema: "public",
                table: "LabBooking");
        }
    }
}
