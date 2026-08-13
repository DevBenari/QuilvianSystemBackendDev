using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableLabHasilBakteri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabHasilBakteris",
                columns: table => new
                {
                    LabHasilBakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabHasilId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    MappingBakteriId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabHasilBakteris", x => x.LabHasilBakteriId);
                    table.ForeignKey(
                        name: "FK_LabHasilBakteris_LabBooking_LabBookingId",
                        column: x => x.LabBookingId,
                        principalSchema: "public",
                        principalTable: "LabBooking",
                        principalColumn: "BookingLabId");
                    table.ForeignKey(
                        name: "FK_LabHasilBakteris_LabHasils_LabHasilId",
                        column: x => x.LabHasilId,
                        principalTable: "LabHasils",
                        principalColumn: "HasilLabId");
                    table.ForeignKey(
                        name: "FK_LabHasilBakteris_MapBakteris_MappingBakteriId",
                        column: x => x.MappingBakteriId,
                        principalTable: "MapBakteris",
                        principalColumn: "MapBakteriId");
                    table.ForeignKey(
                        name: "FK_LabHasilBakteris_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_LabHasilBakteris_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateTable(
                name: "LabNilaiRujukans",
                columns: table => new
                {
                    LabNilaiRujukanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    DariUmur = table.Column<DateOnly>(type: "date", nullable: true),
                    SampaiUmur = table.Column<DateOnly>(type: "date", nullable: true),
                    NilaiMinimum = table.Column<decimal>(type: "numeric", nullable: true),
                    NilaiMaximum = table.Column<decimal>(type: "numeric", nullable: true),
                    NilaiNormal = table.Column<string>(type: "text", nullable: true),
                    HasilNilaiNormal = table.Column<string>(type: "text", nullable: true),
                    StatusNilaiNormal = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LabNilaiRujukans", x => x.LabNilaiRujukanId);
                    table.ForeignKey(
                        name: "FK_LabNilaiRujukans_LabPemeriksaans_PemeriksaanLabId",
                        column: x => x.PemeriksaanLabId,
                        principalTable: "LabPemeriksaans",
                        principalColumn: "PemeriksaanLabId");
                });

            migrationBuilder.CreateTable(
                name: "LabBakteriDetails",
                columns: table => new
                {
                    LabDetailBakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabHasilBakteriId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    AntibiotikId = table.Column<Guid>(type: "uuid", nullable: false),
                    RangeZona = table.Column<string>(type: "text", nullable: true),
                    ZonaMM = table.Column<decimal>(type: "numeric", nullable: true),
                    ResultAntibiotik = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LabBakteriDetails", x => x.LabDetailBakteriId);
                    table.ForeignKey(
                        name: "FK_LabBakteriDetails_LabHasilBakteris_LabHasilBakteriId",
                        column: x => x.LabHasilBakteriId,
                        principalTable: "LabHasilBakteris",
                        principalColumn: "LabHasilBakteriId");
                    table.ForeignKey(
                        name: "FK_LabBakteriDetails_MstAntibiotiks_AntibiotikId",
                        column: x => x.AntibiotikId,
                        principalTable: "MstAntibiotiks",
                        principalColumn: "AntibiotikId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabBakteriDetails_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_LabBakteriDetails_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabBakteriDetails_AntibiotikId",
                table: "LabBakteriDetails",
                column: "AntibiotikId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBakteriDetails_KunjunganId",
                table: "LabBakteriDetails",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBakteriDetails_LabHasilBakteriId",
                table: "LabBakteriDetails",
                column: "LabHasilBakteriId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBakteriDetails_PasienId",
                table: "LabBakteriDetails",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilBakteris_KunjunganId",
                table: "LabHasilBakteris",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilBakteris_LabBookingId",
                table: "LabHasilBakteris",
                column: "LabBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilBakteris_LabHasilId",
                table: "LabHasilBakteris",
                column: "LabHasilId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilBakteris_MappingBakteriId",
                table: "LabHasilBakteris",
                column: "MappingBakteriId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilBakteris_PasienId",
                table: "LabHasilBakteris",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_LabNilaiRujukans_PemeriksaanLabId",
                table: "LabNilaiRujukans",
                column: "PemeriksaanLabId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabBakteriDetails");

            migrationBuilder.DropTable(
                name: "LabNilaiRujukans");

            migrationBuilder.DropTable(
                name: "LabHasilBakteris");
        }
    }
}
