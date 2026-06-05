using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableMasterFilmRadiolog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_LabPersiapanPemeriksaans_LabPersiapanP~",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_MstKunjungan_KunjunganId",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_PdfPasienBaru_PasienId",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabPemeriksaanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPersiapanPemeriksaans_LabPersia~",
                table: "LabPemeriksaanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaanPersiapans_MstLab_LabId",
                table: "LabPemeriksaanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatBendaMedisPasiens");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatBendaMedisPasiens");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatOperasiPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatOperasiPasiens");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatOperasiPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatOperasiPasiens");

            migrationBuilder.CreateTable(
                name: "MstFilm",
                columns: table => new
                {
                    FilmId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaFilm = table.Column<string>(type: "text", nullable: false),
                    UkuranFilm = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstFilm", x => x.FilmId);
                });

            migrationBuilder.CreateTable(
                name: "MstTarifFilm",
                columns: table => new
                {
                    TarifFilmId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilmId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstTarifFilm", x => x.TarifFilmId);
                    table.ForeignKey(
                        name: "FK_MstTarifFilm_MstFilm_FilmId",
                        column: x => x.FilmId,
                        principalTable: "MstFilm",
                        principalColumn: "FilmId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstTarifFilm_MstKelas_KelasId",
                        column: x => x.KelasId,
                        principalSchema: "public",
                        principalTable: "MstKelas",
                        principalColumn: "KelasId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstTarifFilm_FilmId",
                table: "MstTarifFilm",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTarifFilm_KelasId",
                table: "MstTarifFilm",
                column: "KelasId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabJawabanPersiapans",
                column: "PemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_LabPersiapanPemeriksaans_LabPersiapanP~",
                table: "LabJawabanPersiapans",
                column: "LabPersiapanPemeriksaanId",
                principalTable: "LabPersiapanPemeriksaans",
                principalColumn: "LabPersiapanPemeriksaanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_MstKunjungan_KunjunganId",
                table: "LabJawabanPersiapans",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_PdfPasienBaru_PasienId",
                table: "LabJawabanPersiapans",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabPemeriksaanPersiapans",
                column: "PemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPersiapanPemeriksaans_LabPersia~",
                table: "LabPemeriksaanPersiapans",
                column: "LabPersiapanPemeriksaanId",
                principalTable: "LabPersiapanPemeriksaans",
                principalColumn: "LabPersiapanPemeriksaanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaanPersiapans_MstLab_LabId",
                table: "LabPemeriksaanPersiapans",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatBendaMedisPasiens",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatBendaMedisPasiens",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatOperasiPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatOperasiPasiens",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatOperasiPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatOperasiPasiens",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_LabPersiapanPemeriksaans_LabPersiapanP~",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_MstKunjungan_KunjunganId",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabJawabanPersiapans_PdfPasienBaru_PasienId",
                table: "LabJawabanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabPemeriksaanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPersiapanPemeriksaans_LabPersia~",
                table: "LabPemeriksaanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaanPersiapans_MstLab_LabId",
                table: "LabPemeriksaanPersiapans");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatBendaMedisPasiens");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatBendaMedisPasiens");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatOperasiPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatOperasiPasiens");

            migrationBuilder.DropForeignKey(
                name: "FK_RiwayatOperasiPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatOperasiPasiens");

            migrationBuilder.DropTable(
                name: "MstTarifFilm");

            migrationBuilder.DropTable(
                name: "MstFilm");

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabJawabanPersiapans",
                column: "PemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_LabPersiapanPemeriksaans_LabPersiapanP~",
                table: "LabJawabanPersiapans",
                column: "LabPersiapanPemeriksaanId",
                principalTable: "LabPersiapanPemeriksaans",
                principalColumn: "LabPersiapanPemeriksaanId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_MstKunjungan_KunjunganId",
                table: "LabJawabanPersiapans",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_LabJawabanPersiapans_PdfPasienBaru_PasienId",
                table: "LabJawabanPersiapans",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                table: "LabPemeriksaanPersiapans",
                column: "PemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaanPersiapans_LabPersiapanPemeriksaans_LabPersia~",
                table: "LabPemeriksaanPersiapans",
                column: "LabPersiapanPemeriksaanId",
                principalTable: "LabPersiapanPemeriksaans",
                principalColumn: "LabPersiapanPemeriksaanId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaanPersiapans_MstLab_LabId",
                table: "LabPemeriksaanPersiapans",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatBendaMedisPasiens",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatBendaMedisPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatBendaMedisPasiens",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatOperasiPasiens_MstKunjungan_KunjunganId",
                table: "RiwayatOperasiPasiens",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_RiwayatOperasiPasiens_PdfPasienBaru_PasienId",
                table: "RiwayatOperasiPasiens",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }
    }
}
