using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addparamhasilspesimen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabHasilSpecimens_SpecimenJeniss_JenisSpecimenId",
                table: "LabHasilSpecimens");

            migrationBuilder.DropIndex(
                name: "IX_LabHasilSpecimens_JenisSpecimenId",
                table: "LabHasilSpecimens");

            migrationBuilder.DropColumn(
                name: "JenisSpecimenId",
                table: "LabHasilSpecimens");

            migrationBuilder.AddColumn<string>(
                name: "BahanPemeriksaanLainnya",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DokterLuarRS",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeteranganBahanPemeriksaan",
                table: "LabHasils",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BahanPemeriksaanLainnya",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusHasil",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabHasilSpecimenJenis",
                columns: table => new
                {
                    LabHasilSpecimenJenisId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabHasilSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisSpecimenId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabHasilSpecimenJenis", x => x.LabHasilSpecimenJenisId);
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimenJenis_LabHasilSpecimens_LabHasilSpecimenId",
                        column: x => x.LabHasilSpecimenId,
                        principalTable: "LabHasilSpecimens",
                        principalColumn: "LabHasilSpecimenId");
                    table.ForeignKey(
                        name: "FK_LabHasilSpecimenJenis_SpecimenJeniss_JenisSpecimenId",
                        column: x => x.JenisSpecimenId,
                        principalTable: "SpecimenJeniss",
                        principalColumn: "JenisSpecimenId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimenJenis_JenisSpecimenId",
                table: "LabHasilSpecimenJenis",
                column: "JenisSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimenJenis_LabHasilSpecimenId",
                table: "LabHasilSpecimenJenis",
                column: "LabHasilSpecimenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabHasilSpecimenJenis");

            migrationBuilder.DropColumn(
                name: "BahanPemeriksaanLainnya",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "DokterLuarRS",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "KeteranganBahanPemeriksaan",
                table: "LabHasils");

            migrationBuilder.DropColumn(
                name: "BahanPemeriksaanLainnya",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "StatusHasil",
                table: "LabHasilDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "JenisSpecimenId",
                table: "LabHasilSpecimens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabHasilSpecimens_JenisSpecimenId",
                table: "LabHasilSpecimens",
                column: "JenisSpecimenId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabHasilSpecimens_SpecimenJeniss_JenisSpecimenId",
                table: "LabHasilSpecimens",
                column: "JenisSpecimenId",
                principalTable: "SpecimenJeniss",
                principalColumn: "JenisSpecimenId");
        }
    }
}
