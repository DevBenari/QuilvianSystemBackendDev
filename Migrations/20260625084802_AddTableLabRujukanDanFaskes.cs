using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableLabRujukanDanFaskes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstFaskesRujukan",
                columns: table => new
                {
                    FaskesRujukanId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaFaskesRujukan = table.Column<string>(type: "text", nullable: true),
                    AlamatFaskesRujukan = table.Column<string>(type: "text", nullable: true),
                    NoTelpFaskesRujukan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstFaskesRujukan", x => x.FaskesRujukanId);
                });

            migrationBuilder.CreateTable(
                name: "LabRujukans",
                columns: table => new
                {
                    LabRujukanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArahRujukan = table.Column<string>(type: "text", nullable: true),
                    FaskesRujukanId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterPerujuk = table.Column<string>(type: "text", nullable: true),
                    TglRujukan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LabRujukans", x => x.LabRujukanId);
                    table.ForeignKey(
                        name: "FK_LabRujukans_MstFaskesRujukan_FaskesRujukanId",
                        column: x => x.FaskesRujukanId,
                        principalTable: "MstFaskesRujukan",
                        principalColumn: "FaskesRujukanId");
                    table.ForeignKey(
                        name: "FK_LabRujukans_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_LabRujukans_MstLab_LabId",
                        column: x => x.LabId,
                        principalSchema: "public",
                        principalTable: "MstLab",
                        principalColumn: "LabId");
                    table.ForeignKey(
                        name: "FK_LabRujukans_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabRujukans_FaskesRujukanId",
                table: "LabRujukans",
                column: "FaskesRujukanId");

            migrationBuilder.CreateIndex(
                name: "IX_LabRujukans_KunjunganId",
                table: "LabRujukans",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabRujukans_LabId",
                table: "LabRujukans",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabRujukans_PasienId",
                table: "LabRujukans",
                column: "PasienId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabRujukans");

            migrationBuilder.DropTable(
                name: "MstFaskesRujukan");
        }
    }
}
