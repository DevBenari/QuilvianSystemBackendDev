using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addKolomLabBookingDokterPemeriksa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstLab_LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "Diagnosa",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.RenameColumn(
                name: "Satuan",
                schema: "public",
                table: "LabBookingDetail",
                newName: "QtyOrder");

            migrationBuilder.RenameColumn(
                name: "StatusPembayaran",
                schema: "public",
                table: "LabBooking",
                newName: "IsLunas");

            migrationBuilder.RenameColumn(
                name: "DokterId",
                schema: "public",
                table: "LabBooking",
                newName: "DokterPemeriksaId");

            migrationBuilder.RenameIndex(
                name: "IX_LabBooking_StatusPembayaran",
                schema: "public",
                table: "LabBooking",
                newName: "IX_LabBooking_IsLunas");

            migrationBuilder.RenameIndex(
                name: "IX_LabBooking_DokterId_CreateDateTime",
                schema: "public",
                table: "LabBooking",
                newName: "IX_LabBooking_DokterPemeriksaId_CreateDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_LabBooking_DokterId",
                schema: "public",
                table: "LabBooking",
                newName: "IX_LabBooking_DokterPemeriksaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QtyOrder",
                schema: "public",
                table: "LabBookingDetail",
                newName: "Satuan");

            migrationBuilder.RenameColumn(
                name: "IsLunas",
                schema: "public",
                table: "LabBooking",
                newName: "StatusPembayaran");

            migrationBuilder.RenameColumn(
                name: "DokterPemeriksaId",
                schema: "public",
                table: "LabBooking",
                newName: "DokterId");

            migrationBuilder.RenameIndex(
                name: "IX_LabBooking_IsLunas",
                schema: "public",
                table: "LabBooking",
                newName: "IX_LabBooking_StatusPembayaran");

            migrationBuilder.RenameIndex(
                name: "IX_LabBooking_DokterPemeriksaId_CreateDateTime",
                schema: "public",
                table: "LabBooking",
                newName: "IX_LabBooking_DokterId_CreateDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_LabBooking_DokterPemeriksaId",
                schema: "public",
                table: "LabBooking",
                newName: "IX_LabBooking_DokterId");

            migrationBuilder.AddColumn<string>(
                name: "Diagnosa",
                schema: "public",
                table: "LabBookingDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LabId",
                schema: "public",
                table: "LabBookingDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_LabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstLab_LabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
