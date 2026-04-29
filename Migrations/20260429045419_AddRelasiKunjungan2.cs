using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddRelasiKunjungan2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiExcessId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_AsuransiId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienExcessId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_DokterId",
                schema: "public",
                table: "MstKunjungan",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_PasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_PoliklinikId",
                schema: "public",
                table: "MstKunjungan",
                column: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiExcessId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienExcessId",
                principalSchema: "public",
                principalTable: "MstAsuransiPasien",
                principalColumn: "AsuransiPasienId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienId",
                principalSchema: "public",
                principalTable: "MstAsuransiPasien",
                principalColumn: "AsuransiPasienId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstDokter_DokterId",
                schema: "public",
                table: "MstKunjungan",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstKunjungan",
                column: "PoliklinikId",
                principalSchema: "public",
                principalTable: "MstPoliklinik",
                principalColumn: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstDokter_DokterId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_AsuransiId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_DokterId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_PasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_PoliklinikId",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
