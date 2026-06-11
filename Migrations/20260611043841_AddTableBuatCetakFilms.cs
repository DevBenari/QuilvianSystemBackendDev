using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableBuatCetakFilms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "CetakFilm",
                columns: table => new
                {
                    CetakFilmId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterPerujukId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasilLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoOrder = table.Column<string>(type: "text", nullable: true),
                    TglOrder = table.Column<DateOnly>(type: "date", nullable: true),
                    WaktuOrder = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TglSelesai = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalCetakFilm = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    LabHasilHasilLabId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CetakFilm", x => x.CetakFilmId);
                    table.ForeignKey(
                        name: "FK_CetakFilm_LabBooking_LabBookingId",
                        column: x => x.LabBookingId,
                        principalSchema: "public",
                        principalTable: "LabBooking",
                        principalColumn: "BookingLabId");
                    table.ForeignKey(
                        name: "FK_CetakFilm_LabHasils_LabHasilHasilLabId",
                        column: x => x.LabHasilHasilLabId,
                        principalTable: "LabHasils",
                        principalColumn: "HasilLabId");
                    table.ForeignKey(
                        name: "FK_CetakFilm_MstDokter_DokterPerujukId",
                        column: x => x.DokterPerujukId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId");
                    table.ForeignKey(
                        name: "FK_CetakFilm_MstKelas_KelasId",
                        column: x => x.KelasId,
                        principalSchema: "public",
                        principalTable: "MstKelas",
                        principalColumn: "KelasId");
                    table.ForeignKey(
                        name: "FK_CetakFilm_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_CetakFilm_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateTable(
                name: "CetakFilmDetail",
                columns: table => new
                {
                    DetailCetakFilmId = table.Column<Guid>(type: "uuid", nullable: false),
                    CetakFilmId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailHasilLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabBookingDetailId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
                    PemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPemeriksaan = table.Column<string>(type: "text", nullable: true),
                    NoPhoto = table.Column<string>(type: "text", nullable: true),
                    DokterPemeriksaId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaDokterPemeriksa = table.Column<string>(type: "text", nullable: true),
                    PathHasilPhoto = table.Column<string>(type: "text", nullable: true),
                    HasilLab = table.Column<string>(type: "text", nullable: true),
                    HasilLabAI = table.Column<string>(type: "text", nullable: true),
                    FilmId = table.Column<Guid>(type: "uuid", nullable: true),
                    HargaSatuanFilm = table.Column<decimal>(type: "numeric", nullable: true),
                    QtyCetakFilm = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalCetakFilm = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    LabHasilDetailDetailHasilLabId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CetakFilmDetail", x => x.DetailCetakFilmId);
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_CetakFilm_CetakFilmId",
                        column: x => x.CetakFilmId,
                        principalTable: "CetakFilm",
                        principalColumn: "CetakFilmId");
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_LabBookingDetail_LabBookingDetailId",
                        column: x => x.LabBookingDetailId,
                        principalSchema: "public",
                        principalTable: "LabBookingDetail",
                        principalColumn: "DetailBookingLabId");
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_LabHasilDetails_LabHasilDetailDetailHasilLa~",
                        column: x => x.LabHasilDetailDetailHasilLabId,
                        principalTable: "LabHasilDetails",
                        principalColumn: "DetailHasilLabId");
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_LabPemeriksaans_PemeriksaanId",
                        column: x => x.PemeriksaanId,
                        principalTable: "LabPemeriksaans",
                        principalColumn: "PemeriksaanLabId");
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_MstDokter_DokterPemeriksaId",
                        column: x => x.DokterPemeriksaId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId");
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_MstFilm_FilmId",
                        column: x => x.FilmId,
                        principalTable: "MstFilm",
                        principalColumn: "FilmId");
                    table.ForeignKey(
                        name: "FK_CetakFilmDetail_MstLab_LabId",
                        column: x => x.LabId,
                        principalSchema: "public",
                        principalTable: "MstLab",
                        principalColumn: "LabId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilm_DokterPerujukId",
                table: "CetakFilm",
                column: "DokterPerujukId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilm_KelasId",
                table: "CetakFilm",
                column: "KelasId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilm_KunjunganId",
                table: "CetakFilm",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilm_LabBookingId",
                table: "CetakFilm",
                column: "LabBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilm_LabHasilHasilLabId",
                table: "CetakFilm",
                column: "LabHasilHasilLabId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilm_PasienId",
                table: "CetakFilm",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_CetakFilmId",
                table: "CetakFilmDetail",
                column: "CetakFilmId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_DokterPemeriksaId",
                table: "CetakFilmDetail",
                column: "DokterPemeriksaId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_FilmId",
                table: "CetakFilmDetail",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_LabBookingDetailId",
                table: "CetakFilmDetail",
                column: "LabBookingDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_LabHasilDetailDetailHasilLabId",
                table: "CetakFilmDetail",
                column: "LabHasilDetailDetailHasilLabId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_LabId",
                table: "CetakFilmDetail",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_PemeriksaanId",
                table: "CetakFilmDetail",
                column: "PemeriksaanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CetakFilmDetail");

            migrationBuilder.DropTable(
                name: "CetakFilm");
        }
    }
}
