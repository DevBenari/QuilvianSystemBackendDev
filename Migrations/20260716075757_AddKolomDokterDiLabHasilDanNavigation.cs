using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDokterDiLabHasilDanNavigation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DokterKonfirmatorId",
                table: "LabHasils",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterPerujukId",
                table: "LabHasils",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKonfirmatorDPJP",
                table: "LabHasils",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPhoneKonfirmator",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_DokterKonfirmatorId",
                table: "LabHasils",
                column: "DokterKonfirmatorId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_DokterPerujukId",
                table: "LabHasils",
                column: "DokterPerujukId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_KunjunganId",
                table: "LabHasils",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_LabBookingId",
                table: "LabHasils",
                column: "LabBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_LabId",
                table: "LabHasils",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_PenanggungJawabAnalisId",
                table: "LabHasils",
                column: "PenanggungJawabAnalisId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasils_PenanggungJawabId",
                table: "LabHasils",
                column: "PenanggungJawabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_LabBooking_LabBookingId",
                table: "LabHasils",
                column: "LabBookingId",
                principalSchema: "public",
                principalTable: "LabBooking",
                principalColumn: "BookingLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_MstDokter_DokterKonfirmatorId",
                table: "LabHasils",
                column: "DokterKonfirmatorId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_MstDokter_DokterPerujukId",
                table: "LabHasils",
                column: "DokterPerujukId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_MstKunjungan_KunjunganId",
                table: "LabHasils",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_MstLab_LabId",
                table: "LabHasils",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_MstUserActive_PenanggungJawabAnalisId",
                table: "LabHasils",
                column: "PenanggungJawabAnalisId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasils_MstUserActive_PenanggungJawabId",
                table: "LabHasils",
                column: "PenanggungJawabId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_LabBooking_LabBookingId",
                table: "LabHasils");

            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_MstDokter_DokterKonfirmatorId",
                table: "LabHasils");

            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_MstDokter_DokterPerujukId",
                table: "LabHasils");

            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_MstKunjungan_KunjunganId",
                table: "LabHasils");

            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_MstLab_LabId",
                table: "LabHasils");

            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_MstUserActive_PenanggungJawabAnalisId",
                table: "LabHasils");

            migrationBuilder.DropForeignKey(
                name: "FK_LabHasils_MstUserActive_PenanggungJawabId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_DokterKonfirmatorId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_DokterPerujukId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_KunjunganId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_LabBookingId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_LabId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_PenanggungJawabAnalisId",
                table: "LabHasils");

            migrationBuilder.DropIndex(
                name: "IX_LabHasils_PenanggungJawabId",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "DokterKonfirmatorId",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "DokterPerujukId",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "IsKonfirmatorDPJP",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "NoPhoneKonfirmator",
                table: "LabHasils");
        }
    }
}
