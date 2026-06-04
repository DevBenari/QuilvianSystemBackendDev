using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePersiapanRadiolog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DurasiPuasa",
                table: "LabPemeriksaans",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsButuhPersiapan",
                table: "LabPemeriksaans",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabPersiapanPemeriksaans",
                columns: table => new
                {
                    LabPersiapanPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersiapanPemeriksaan = table.Column<string>(type: "text", nullable: true),
                    TipePersiapan = table.Column<string>(type: "text", nullable: true),
                    IsDetailPersiapan = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabPersiapanPemeriksaans", x => x.LabPersiapanPemeriksaanId);
                });

            migrationBuilder.CreateTable(
                name: "RiwayatBendaMedisPasiens",
                columns: table => new
                {
                    RiwayatBendaMedisPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    SumberDataId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSumberData = table.Column<string>(type: "text", nullable: true),
                    NamaBendaMedis = table.Column<string>(type: "text", nullable: true),
                    LokasiBendaMedis = table.Column<string>(type: "text", nullable: true),
                    IsPermanen = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiwayatBendaMedisPasiens", x => x.RiwayatBendaMedisPasienId);
                    table.ForeignKey(
                        name: "FK_RiwayatBendaMedisPasiens_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_RiwayatBendaMedisPasiens_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateTable(
                name: "RiwayatOperasiPasiens",
                columns: table => new
                {
                    RiwayatOperasiPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    SumberDataId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSumberData = table.Column<string>(type: "text", nullable: true),
                    NamaOperasi = table.Column<string>(type: "text", nullable: true),
                    LokasiTubuh = table.Column<string>(type: "text", nullable: true),
                    IndikasiOperasi = table.Column<string>(type: "text", nullable: true),
                    WaktuOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiwayatOperasiPasiens", x => x.RiwayatOperasiPasienId);
                    table.ForeignKey(
                        name: "FK_RiwayatOperasiPasiens_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_RiwayatOperasiPasiens_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateTable(
                name: "LabJawabanPersiapans",
                columns: table => new
                {
                    LabJawabanPersiapanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabPersiapanPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsJawabanPersiapan = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabJawabanPersiapans", x => x.LabJawabanPersiapanId);
                    table.ForeignKey(
                        name: "FK_LabJawabanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                        column: x => x.PemeriksaanLabId,
                        principalTable: "LabPemeriksaans",
                        principalColumn: "PemeriksaanLabId");
                    table.ForeignKey(
                        name: "FK_LabJawabanPersiapans_LabPersiapanPemeriksaans_LabPersiapanP~",
                        column: x => x.LabPersiapanPemeriksaanId,
                        principalTable: "LabPersiapanPemeriksaans",
                        principalColumn: "LabPersiapanPemeriksaanId");
                    table.ForeignKey(
                        name: "FK_LabJawabanPersiapans_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_LabJawabanPersiapans_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateTable(
                name: "LabPemeriksaanPersiapans",
                columns: table => new
                {
                    LabPemeriksaanPersiapanId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabPersiapanPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabPemeriksaanPersiapans", x => x.LabPemeriksaanPersiapanId);
                    table.ForeignKey(
                        name: "FK_LabPemeriksaanPersiapans_LabPemeriksaans_PemeriksaanLabId",
                        column: x => x.PemeriksaanLabId,
                        principalTable: "LabPemeriksaans",
                        principalColumn: "PemeriksaanLabId");
                    table.ForeignKey(
                        name: "FK_LabPemeriksaanPersiapans_LabPersiapanPemeriksaans_LabPersia~",
                        column: x => x.LabPersiapanPemeriksaanId,
                        principalTable: "LabPersiapanPemeriksaans",
                        principalColumn: "LabPersiapanPemeriksaanId");
                    table.ForeignKey(
                        name: "FK_LabPemeriksaanPersiapans_MstLab_LabId",
                        column: x => x.LabId,
                        principalSchema: "public",
                        principalTable: "MstLab",
                        principalColumn: "LabId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabJawabanPersiapans_KunjunganId",
                table: "LabJawabanPersiapans",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabJawabanPersiapans_LabPersiapanPemeriksaanId",
                table: "LabJawabanPersiapans",
                column: "LabPersiapanPemeriksaanId");

            migrationBuilder.CreateIndex(
                name: "IX_LabJawabanPersiapans_PasienId",
                table: "LabJawabanPersiapans",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_LabJawabanPersiapans_PemeriksaanLabId",
                table: "LabJawabanPersiapans",
                column: "PemeriksaanLabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPemeriksaanPersiapans_LabId",
                table: "LabPemeriksaanPersiapans",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPemeriksaanPersiapans_LabPersiapanPemeriksaanId",
                table: "LabPemeriksaanPersiapans",
                column: "LabPersiapanPemeriksaanId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPemeriksaanPersiapans_PemeriksaanLabId",
                table: "LabPemeriksaanPersiapans",
                column: "PemeriksaanLabId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatBendaMedisPasiens_KunjunganId",
                table: "RiwayatBendaMedisPasiens",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatBendaMedisPasiens_PasienId",
                table: "RiwayatBendaMedisPasiens",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatOperasiPasiens_KunjunganId",
                table: "RiwayatOperasiPasiens",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatOperasiPasiens_PasienId",
                table: "RiwayatOperasiPasiens",
                column: "PasienId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabJawabanPersiapans");

            migrationBuilder.DropTable(
                name: "LabPemeriksaanPersiapans");

            migrationBuilder.DropTable(
                name: "RiwayatBendaMedisPasiens");

            migrationBuilder.DropTable(
                name: "RiwayatOperasiPasiens");

            migrationBuilder.DropTable(
                name: "LabPersiapanPemeriksaans");

            migrationBuilder.DropColumn(
                name: "DurasiPuasa",
                table: "LabPemeriksaans");

            migrationBuilder.DropColumn(
                name: "IsButuhPersiapan",
                table: "LabPemeriksaans");
        }
    }
}
