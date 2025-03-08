using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class poli : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstSubPoli_SubPoliId",
                table: "DokterPolis");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_SubPoliId",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "KewarganegaraanId",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "MaxPasien",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "KodeDokterSubPoli",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "NamaSubPoli",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "SubPoliId",
                table: "DokterPolis");

            migrationBuilder.AddColumn<string>(
                name: "Kewarganegaraan",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "JumlahMaxPasien",
                schema: "public",
                table: "MstSubPoli",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JumlahMaxPasien",
                schema: "public",
                table: "MstPoliklinik",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MstDokterSubPoli",
                schema: "public",
                columns: table => new
                {
                    DokterSubPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaDokter = table.Column<string>(type: "text", nullable: false),
                    KodeDokterSubPoli = table.Column<string>(type: "text", nullable: true),
                    SubPoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSubPoli = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstDokterSubPoli", x => x.DokterSubPoliId);
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstAsuransi_AsuransiId",
                        column: x => x.AsuransiId,
                        principalSchema: "public",
                        principalTable: "MstAsuransi",
                        principalColumn: "AsuransiId");
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstSubPoli_SubPoliId",
                        column: x => x.SubPoliId,
                        principalSchema: "public",
                        principalTable: "MstSubPoli",
                        principalColumn: "SubPoliId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_AsuransiId",
                table: "DokterPolis",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_AsuransiId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_DokterId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_SubPoliId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "SubPoliId");

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPolis_MstAsuransi_AsuransiId",
                table: "DokterPolis",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId",
                principalSchema: "public",
                principalTable: "MstDokterSubPoli",
                principalColumn: "DokterSubPoliId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstAsuransi_AsuransiId",
                table: "DokterPolis");

            migrationBuilder.DropForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropTable(
                name: "MstDokterSubPoli",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstJadwalPraktek_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_AsuransiId",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "Kewarganegaraan",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "JumlahMaxPasien",
                schema: "public",
                table: "MstSubPoli");

            migrationBuilder.DropColumn(
                name: "JumlahMaxPasien",
                schema: "public",
                table: "MstPoliklinik");

            migrationBuilder.DropColumn(
                name: "DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.AddColumn<Guid>(
                name: "KewarganegaraanId",
                schema: "public",
                table: "PdfPasienBaru",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPasien",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "KodeDokterSubPoli",
                table: "DokterPolis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaSubPoli",
                table: "DokterPolis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubPoliId",
                table: "DokterPolis",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_SubPoliId",
                table: "DokterPolis",
                column: "SubPoliId");

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPolis_MstSubPoli_SubPoliId",
                table: "DokterPolis",
                column: "SubPoliId",
                principalSchema: "public",
                principalTable: "MstSubPoli",
                principalColumn: "SubPoliId");
        }
    }
}
