using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationPropPagedDokter3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PoliklinikId",
                table: "DokterPolis",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterAsuransi_DokterId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_DokterId",
                table: "DokterPolis",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_PoliklinikId",
                table: "DokterPolis",
                column: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPolis_MstDokter_DokterId",
                table: "DokterPolis",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPolis_MstPoliklinik_PoliklinikId",
                table: "DokterPolis",
                column: "PoliklinikId",
                principalSchema: "public",
                principalTable: "MstPoliklinik",
                principalColumn: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterAsuransi_MstDokter_DokterId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstJadwalPraktek_MstDokter_DokterId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstDokter_DokterId",
                table: "DokterPolis");

            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstPoliklinik_PoliklinikId",
                table: "DokterPolis");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterAsuransi_MstDokter_DokterId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropForeignKey(
                name: "FK_MstJadwalPraktek_MstDokter_DokterId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropIndex(
                name: "IX_MstJadwalPraktek_DokterId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropIndex(
                name: "IX_MstDokterAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstDokterAsuransi_DokterId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_DokterId",
                table: "DokterPolis");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_PoliklinikId",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "DokterId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "PoliklinikId",
                table: "DokterPolis");
        }
    }
}
