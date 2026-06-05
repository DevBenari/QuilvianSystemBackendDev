using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddPindahKolomLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_NoOrder",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "NoOrder",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "WaktuVerifikasi",
                schema: "public",
                table: "LabBookingDetail");

            // Tambahkan kolom DokterPemeriksaId dulu sebelum CreateIndex dan AddForeignKey
            migrationBuilder.AddColumn<Guid>(
                name: "DokterPemeriksaId",
                schema: "public",
                table: "LabBookingDetail",
                type: "uuid",
                nullable: true);

            // IsPasienPersiapan boolean, bukan string
            migrationBuilder.AddColumn<bool>(
                name: "IsPasienPersiapan",
                schema: "public",
                table: "LabBooking",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_DokterPemeriksaId",
                schema: "public",
                table: "LabBookingDetail",
                column: "DokterPemeriksaId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstDokter_DokterPemeriksaId",
                schema: "public",
                table: "LabBookingDetail",
                column: "DokterPemeriksaId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstDokter_DokterPemeriksaId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_DokterPemeriksaId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "DokterPemeriksaId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "IsPasienPersiapan",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.AddColumn<string>(
                name: "NoOrder",
                schema: "public",
                table: "LabBookingDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaktuVerifikasi",
                schema: "public",
                table: "LabBookingDetail",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_NoOrder",
                schema: "public",
                table: "LabBookingDetail",
                column: "NoOrder");
        }
    }
}