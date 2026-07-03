using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class NavigationPropDiAsuransidanLabBatal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LabBookingDetailDetailBookingLabId",
                table: "LabBookingBatals",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstAsuransiPasien_AsuransiId",
                schema: "public",
                table: "MstAsuransiPasien",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAsuransiPasien_PasienId",
                schema: "public",
                table: "MstAsuransiPasien",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingBatals_LabBookingDetailDetailBookingLabId",
                table: "LabBookingBatals",
                column: "LabBookingDetailDetailBookingLabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingBatals_LabBookingId",
                table: "LabBookingBatals",
                column: "LabBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingBedRanap_BedId",
                schema: "public",
                table: "BookingBedRanap",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingBedRanap_KamarId",
                schema: "public",
                table: "BookingBedRanap",
                column: "KamarId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingBedRanap_Beds_BedId",
                schema: "public",
                table: "BookingBedRanap",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "BedId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingBedRanap_Kamars_KamarId",
                schema: "public",
                table: "BookingBedRanap",
                column: "KamarId",
                principalTable: "Kamars",
                principalColumn: "KamarId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingBatals_LabBooking_LabBookingId",
                table: "LabBookingBatals",
                column: "LabBookingId",
                principalSchema: "public",
                principalTable: "LabBooking",
                principalColumn: "BookingLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingBatals_LabBookingDetail_LabBookingDetailDetailBoo~",
                table: "LabBookingBatals",
                column: "LabBookingDetailDetailBookingLabId",
                principalSchema: "public",
                principalTable: "LabBookingDetail",
                principalColumn: "DetailBookingLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstAsuransiPasien_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstAsuransiPasien",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstAsuransiPasien_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstAsuransiPasien",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingBedRanap_Beds_BedId",
                schema: "public",
                table: "BookingBedRanap");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingBedRanap_Kamars_KamarId",
                schema: "public",
                table: "BookingBedRanap");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingBatals_LabBooking_LabBookingId",
                table: "LabBookingBatals");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingBatals_LabBookingDetail_LabBookingDetailDetailBoo~",
                table: "LabBookingBatals");

            migrationBuilder.DropForeignKey(
                name: "FK_MstAsuransiPasien_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstAsuransiPasien");

            migrationBuilder.DropForeignKey(
                name: "FK_MstAsuransiPasien_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstAsuransiPasien");

            migrationBuilder.DropIndex(
                name: "IX_MstAsuransiPasien_AsuransiId",
                schema: "public",
                table: "MstAsuransiPasien");

            migrationBuilder.DropIndex(
                name: "IX_MstAsuransiPasien_PasienId",
                schema: "public",
                table: "MstAsuransiPasien");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingBatals_LabBookingDetailDetailBookingLabId",
                table: "LabBookingBatals");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingBatals_LabBookingId",
                table: "LabBookingBatals");

            migrationBuilder.DropIndex(
                name: "IX_BookingBedRanap_BedId",
                schema: "public",
                table: "BookingBedRanap");

            migrationBuilder.DropIndex(
                name: "IX_BookingBedRanap_KamarId",
                schema: "public",
                table: "BookingBedRanap");

            migrationBuilder.DropColumn(
                name: "LabBookingDetailDetailBookingLabId",
                table: "LabBookingBatals");
        }
    }
}
