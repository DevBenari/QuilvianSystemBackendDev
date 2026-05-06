using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addICollectionPemintaanUnitdanFarmasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaianDetails_AlatPemakaians_AlatPemakaianPemakaianA~",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaians_MstKunjungan_KunjunganId",
                table: "AlatPemakaians");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaians_PdfPasienBaru_PasienId",
                table: "AlatPemakaians");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaianDetails_AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropColumn(
                name: "AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails");

            migrationBuilder.CreateIndex(
                name: "IX_PermintaanUnits_TujuanUnitId",
                table: "PermintaanUnits",
                column: "TujuanUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PermintaanUnits_UnitId",
                table: "PermintaanUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PenerimaanUnits_UnitId",
                table: "PenerimaanUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmasiRJ_KonversiSatuanId",
                schema: "public",
                table: "FarmasiRJ",
                column: "KonversiSatuanId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmasiRJ_ObatId",
                schema: "public",
                table: "FarmasiRJ",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPermintaanUnits_ObatId",
                table: "DetailPermintaanUnits",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPermintaanUnits_PermintaanUnitId",
                table: "DetailPermintaanUnits",
                column: "PermintaanUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenerimaanUnits_ObatId",
                table: "DetailPenerimaanUnits",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenerimaanUnits_PenerimaanUnitId",
                table: "DetailPenerimaanUnits",
                column: "PenerimaanUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaianDetails_KelasId",
                table: "AlatPemakaianDetails",
                column: "KelasId");

            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaianDetails_PemakaianAlatId",
                table: "AlatPemakaianDetails",
                column: "PemakaianAlatId");

            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaianDetails_PeralatanId",
                table: "AlatPemakaianDetails",
                column: "PeralatanId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaianDetails_AlatPemakaians_PemakaianAlatId",
                table: "AlatPemakaianDetails",
                column: "PemakaianAlatId",
                principalTable: "AlatPemakaians",
                principalColumn: "PemakaianAlatId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaianDetails_MstKelas_KelasId",
                table: "AlatPemakaianDetails",
                column: "KelasId",
                principalSchema: "public",
                principalTable: "MstKelas",
                principalColumn: "KelasId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaianDetails_MstPeralatan_PeralatanId",
                table: "AlatPemakaianDetails",
                column: "PeralatanId",
                principalSchema: "public",
                principalTable: "MstPeralatan",
                principalColumn: "PeralatanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaians_MstKunjungan_KunjunganId",
                table: "AlatPemakaians",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaians_PdfPasienBaru_PasienId",
                table: "AlatPemakaians",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailPenerimaanUnits_MstObat_ObatId",
                table: "DetailPenerimaanUnits",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailPenerimaanUnits_PenerimaanUnits_PenerimaanUnitId",
                table: "DetailPenerimaanUnits",
                column: "PenerimaanUnitId",
                principalTable: "PenerimaanUnits",
                principalColumn: "PenerimaanUnitId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailPermintaanUnits_MstObat_ObatId",
                table: "DetailPermintaanUnits",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailPermintaanUnits_PermintaanUnits_PermintaanUnitId",
                table: "DetailPermintaanUnits",
                column: "PermintaanUnitId",
                principalTable: "PermintaanUnits",
                principalColumn: "PermintaanUnitId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FarmasiRJ_MstKonversiSatuan_KonversiSatuanId",
                schema: "public",
                table: "FarmasiRJ",
                column: "KonversiSatuanId",
                principalSchema: "public",
                principalTable: "MstKonversiSatuan",
                principalColumn: "KonversiSatuanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FarmasiRJ_MstObat_ObatId",
                schema: "public",
                table: "FarmasiRJ",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PenerimaanUnits_Hrd_InstalasiUnit_UnitId",
                table: "PenerimaanUnits",
                column: "UnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PermintaanUnits_Hrd_InstalasiUnit_TujuanUnitId",
                table: "PermintaanUnits",
                column: "TujuanUnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PermintaanUnits_Hrd_InstalasiUnit_UnitId",
                table: "PermintaanUnits",
                column: "UnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaianDetails_AlatPemakaians_PemakaianAlatId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaianDetails_MstKelas_KelasId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaianDetails_MstPeralatan_PeralatanId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaians_MstKunjungan_KunjunganId",
                table: "AlatPemakaians");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaians_PdfPasienBaru_PasienId",
                table: "AlatPemakaians");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailPenerimaanUnits_MstObat_ObatId",
                table: "DetailPenerimaanUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailPenerimaanUnits_PenerimaanUnits_PenerimaanUnitId",
                table: "DetailPenerimaanUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailPermintaanUnits_MstObat_ObatId",
                table: "DetailPermintaanUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailPermintaanUnits_PermintaanUnits_PermintaanUnitId",
                table: "DetailPermintaanUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_FarmasiRJ_MstKonversiSatuan_KonversiSatuanId",
                schema: "public",
                table: "FarmasiRJ");

            migrationBuilder.DropForeignKey(
                name: "FK_FarmasiRJ_MstObat_ObatId",
                schema: "public",
                table: "FarmasiRJ");

            migrationBuilder.DropForeignKey(
                name: "FK_PenerimaanUnits_Hrd_InstalasiUnit_UnitId",
                table: "PenerimaanUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_PermintaanUnits_Hrd_InstalasiUnit_TujuanUnitId",
                table: "PermintaanUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_PermintaanUnits_Hrd_InstalasiUnit_UnitId",
                table: "PermintaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_PermintaanUnits_TujuanUnitId",
                table: "PermintaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_PermintaanUnits_UnitId",
                table: "PermintaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_PenerimaanUnits_UnitId",
                table: "PenerimaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_FarmasiRJ_KonversiSatuanId",
                schema: "public",
                table: "FarmasiRJ");

            migrationBuilder.DropIndex(
                name: "IX_FarmasiRJ_ObatId",
                schema: "public",
                table: "FarmasiRJ");

            migrationBuilder.DropIndex(
                name: "IX_DetailPermintaanUnits_ObatId",
                table: "DetailPermintaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_DetailPermintaanUnits_PermintaanUnitId",
                table: "DetailPermintaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_DetailPenerimaanUnits_ObatId",
                table: "DetailPenerimaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_DetailPenerimaanUnits_PenerimaanUnitId",
                table: "DetailPenerimaanUnits");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaianDetails_KelasId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaianDetails_PemakaianAlatId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaianDetails_PeralatanId",
                table: "AlatPemakaianDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaianDetails_AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails",
                column: "AlatPemakaianPemakaianAlatId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaianDetails_AlatPemakaians_AlatPemakaianPemakaianA~",
                table: "AlatPemakaianDetails",
                column: "AlatPemakaianPemakaianAlatId",
                principalTable: "AlatPemakaians",
                principalColumn: "PemakaianAlatId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaians_MstKunjungan_KunjunganId",
                table: "AlatPemakaians",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaians_PdfPasienBaru_PasienId",
                table: "AlatPemakaians",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }
    }
}
