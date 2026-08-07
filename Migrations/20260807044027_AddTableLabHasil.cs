using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableLabHasil : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabHasilSpecimens",
                columns: table => new
                {
                    LabHasilSpecimenId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabHasilId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsalSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabHasilSpecimens", x => x.LabHasilSpecimenId);
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimens_LabHasils_LabHasilId",
                        column: x => x.LabHasilId,
                        principalTable: "LabHasils",
                        principalColumn: "HasilLabId");
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimens_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID");
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimens_MstSpecimenAsal_AsalSpecimenId",
                        column: x => x.AsalSpecimenId,
                        principalSchema: "public",
                        principalTable: "MstSpecimenAsal",
                        principalColumn: "SpecimenAsalId");
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimens_PdfPasienBaru_PasienId",
                        column: x => x.PasienId,
                        principalSchema: "public",
                        principalTable: "PdfPasienBaru",
                        principalColumn: "PendaftaranPasienBaruId");
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimens_SpecimenJeniss_JenisSpecimenId",
                        column: x => x.JenisSpecimenId,
                        principalTable: "SpecimenJeniss",
                        principalColumn: "JenisSpecimenId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimens_AsalSpecimenId",
                table: "LabHasilSpecimens",
                column: "AsalSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimens_JenisSpecimenId",
                table: "LabHasilSpecimens",
                column: "JenisSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimens_KunjunganId",
                table: "LabHasilSpecimens",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimens_LabHasilId",
                table: "LabHasilSpecimens",
                column: "LabHasilId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimens_PasienId",
                table: "LabHasilSpecimens",
                column: "PasienId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabHasilSpecimens");
        }
    }
}
