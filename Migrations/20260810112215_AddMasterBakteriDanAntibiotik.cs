using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddMasterBakteriDanAntibiotik : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BahanPemeriksaanLainnya",
                table: "LabHasilDetails",
                newName: "HasilPemeriksaan");

            migrationBuilder.CreateTable(
                name: "LabHasilSetBakteris",
                columns: table => new
                {
                    LabHasilSetBakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabHasilId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsalSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabHasilSetBakteris", x => x.LabHasilSetBakteriId);
                    table.ForeignKey(
                        name: "FK_LabHasilSetBakteris_LabHasils_LabHasilId",
                        column: x => x.LabHasilId,
                        principalTable: "LabHasils",
                        principalColumn: "HasilLabId");
                    table.ForeignKey(
                        name: "FK_LabHasilSetBakteris_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_LabHasilSetBakteris_MstSpecimenAsal_AsalSpecimenId",
                        column: x => x.AsalSpecimenId,
                        principalSchema: "public",
                        principalTable: "MstSpecimenAsal",
                        principalColumn: "SpecimenAsalId");
                    table.ForeignKey(
                        name: "FK_LabHasilSetBakteris_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                    table.ForeignKey(
                        name: "FK_LabHasilSetBakteris_SpecimenJeniss_JenisSpecimenId",
                        column: x => x.JenisSpecimenId,
                        principalTable: "SpecimenJeniss",
                        principalColumn: "JenisSpecimenId");
                });

            migrationBuilder.CreateTable(
                name: "MstAntibiotiks",
                columns: table => new
                {
                    AntibiotikId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeAntibiotik = table.Column<string>(type: "text", nullable: true),
                    Microgram = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstAntibiotiks", x => x.AntibiotikId);
                });

            migrationBuilder.CreateTable(
                name: "MstBakteris",
                columns: table => new
                {
                    BakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBakteri = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstBakteris", x => x.BakteriId);
                });

            migrationBuilder.CreateTable(
                name: "MstSubBakteris",
                columns: table => new
                {
                    SubBakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeSubBakteri = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSubBakteris", x => x.SubBakteriId);
                });

            migrationBuilder.CreateTable(
                name: "MapAntibiotikSubBakteris",
                columns: table => new
                {
                    MapAntibiotikSubBakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubBakteriId = table.Column<Guid>(type: "uuid", nullable: true),
                    AntibiotikId = table.Column<Guid>(type: "uuid", nullable: true),
                    NormalMin = table.Column<decimal>(type: "numeric", nullable: true),
                    NormalMax = table.Column<decimal>(type: "numeric", nullable: true),
                    CriticalMin = table.Column<decimal>(type: "numeric", nullable: true),
                    CriticalMax = table.Column<decimal>(type: "numeric", nullable: true),
                    UrutanTampil = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MapAntibiotikSubBakteris", x => x.MapAntibiotikSubBakteriId);
                    table.ForeignKey(
                        name: "FK_MapAntibiotikSubBakteris_MstAntibiotiks_AntibiotikId",
                        column: x => x.AntibiotikId,
                        principalTable: "MstAntibiotiks",
                        principalColumn: "AntibiotikId");
                    table.ForeignKey(
                        name: "FK_MapAntibiotikSubBakteris_MstSubBakteris_SubBakteriId",
                        column: x => x.SubBakteriId,
                        principalTable: "MstSubBakteris",
                        principalColumn: "SubBakteriId");
                });

            migrationBuilder.CreateTable(
                name: "MapBakteris",
                columns: table => new
                {
                    MapBakteriId = table.Column<Guid>(type: "uuid", nullable: false),
                    BakteriId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubBakteriId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MapBakteris", x => x.MapBakteriId);
                    table.ForeignKey(
                        name: "FK_MapBakteris_MstBakteris_BakteriId",
                        column: x => x.BakteriId,
                        principalTable: "MstBakteris",
                        principalColumn: "BakteriId");
                    table.ForeignKey(
                        name: "FK_MapBakteris_MstSubBakteris_SubBakteriId",
                        column: x => x.SubBakteriId,
                        principalTable: "MstSubBakteris",
                        principalColumn: "SubBakteriId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSetBakteris_AsalSpecimenId",
                table: "LabHasilSetBakteris",
                column: "AsalSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSetBakteris_JenisSpecimenId",
                table: "LabHasilSetBakteris",
                column: "JenisSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSetBakteris_KunjunganId",
                table: "LabHasilSetBakteris",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSetBakteris_LabHasilId",
                table: "LabHasilSetBakteris",
                column: "LabHasilId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSetBakteris_PasienId",
                table: "LabHasilSetBakteris",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_MapAntibiotikSubBakteris_AntibiotikId",
                table: "MapAntibiotikSubBakteris",
                column: "AntibiotikId");

            migrationBuilder.CreateIndex(
                name: "IX_MapAntibiotikSubBakteris_SubBakteriId",
                table: "MapAntibiotikSubBakteris",
                column: "SubBakteriId");

            migrationBuilder.CreateIndex(
                name: "IX_MapBakteris_BakteriId",
                table: "MapBakteris",
                column: "BakteriId");

            migrationBuilder.CreateIndex(
                name: "IX_MapBakteris_SubBakteriId",
                table: "MapBakteris",
                column: "SubBakteriId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabHasilSetBakteris");

            migrationBuilder.DropTable(
                name: "MapAntibiotikSubBakteris");

            migrationBuilder.DropTable(
                name: "MapBakteris");

            migrationBuilder.DropTable(
                name: "MstAntibiotiks");

            migrationBuilder.DropTable(
                name: "MstBakteris");

            migrationBuilder.DropTable(
                name: "MstSubBakteris");

            migrationBuilder.RenameColumn(
                name: "HasilPemeriksaan",
                table: "LabHasilDetails",
                newName: "BahanPemeriksaanLainnya");
        }
    }
}
