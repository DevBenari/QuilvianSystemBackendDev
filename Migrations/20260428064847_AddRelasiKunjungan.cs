using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddRelasiKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstResep_AsuransiId",
                schema: "public",
                table: "MstResep",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResep_DokterId",
                schema: "public",
                table: "MstResep",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResep_KunjunganId",
                schema: "public",
                table: "MstResep",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResep_PasienId",
                schema: "public",
                table: "MstResep",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResep_PetugasFarmasiId",
                schema: "public",
                table: "MstResep",
                column: "PetugasFarmasiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResep_PoliklinikId",
                schema: "public",
                table: "MstResep",
                column: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResep_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstResep",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResep_MstDokter_DokterId",
                schema: "public",
                table: "MstResep",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResep_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MstResep",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResep_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstResep",
                column: "PoliklinikId",
                principalSchema: "public",
                principalTable: "MstPoliklinik",
                principalColumn: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResep_MstUserActive_PetugasFarmasiId",
                schema: "public",
                table: "MstResep",
                column: "PetugasFarmasiId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResep_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstResep",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstResep_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResep_MstDokter_DokterId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResep_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResep_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResep_MstUserActive_PetugasFarmasiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResep_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropIndex(
                name: "IX_MstResep_AsuransiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropIndex(
                name: "IX_MstResep_DokterId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropIndex(
                name: "IX_MstResep_KunjunganId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropIndex(
                name: "IX_MstResep_PasienId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropIndex(
                name: "IX_MstResep_PetugasFarmasiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropIndex(
                name: "IX_MstResep_PoliklinikId",
                schema: "public",
                table: "MstResep");
        }
    }
}
