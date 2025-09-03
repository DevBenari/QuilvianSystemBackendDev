using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableResumePulang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstResepTemplate_MstObat_ObatId",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstResepTemplate_ObatId",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "InteraturObat",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "ObatId",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "Qty",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "Signa",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "SignaTambahan",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.CreateTable(
                name: "ResepTemplateDetails",
                columns: table => new
                {
                    ResepTemplateDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    RacikanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    TakaranDosis = table.Column<decimal>(type: "numeric", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    JenisObat = table.Column<string>(type: "text", nullable: true),
                    HargaObat = table.Column<decimal>(type: "numeric", nullable: true),
                    StatusCoverObat = table.Column<bool>(type: "boolean", nullable: true),
                    IsRacikan = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_ResepTemplateDetails", x => x.ResepTemplateDetailId);
                });

            migrationBuilder.CreateTable(
                name: "ResumePulangs",
                columns: table => new
                {
                    ResumeMedisId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingBedId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailIcdid = table.Column<Guid>(type: "uuid", nullable: true),
                    IndikasiRanap = table.Column<string>(type: "text", nullable: true),
                    RiwayatPenyakit = table.Column<string>(type: "text", nullable: true),
                    PemeriksaanFisik = table.Column<string>(type: "text", nullable: true),
                    HasilLab = table.Column<string>(type: "text", nullable: true),
                    DiagnosaUtama = table.Column<string>(type: "text", nullable: true),
                    IsOperasi = table.Column<bool>(type: "boolean", nullable: true),
                    WaktuKontrol = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SaranPemeriksaan = table.Column<string>(type: "text", nullable: true),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    TerapiMedis = table.Column<string>(type: "text", nullable: true),
                    HasilKonsultasi = table.Column<string>(type: "text", nullable: true),
                    PendingResult = table.Column<bool>(type: "boolean", nullable: true),
                    Diet = table.Column<string>(type: "text", nullable: true),
                    IsiEdukasi = table.Column<string>(type: "text", nullable: true),
                    KondisiPulang = table.Column<string>(type: "text", nullable: true),
                    TakeHomeResult = table.Column<string>(type: "text", nullable: true),
                    IntruksiPulang = table.Column<string>(type: "text", nullable: true),
                    TtdPenerima = table.Column<string>(type: "text", nullable: true),
                    TtdPemberi = table.Column<string>(type: "text", nullable: true),
                    StatusResume = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_ResumePulangs", x => x.ResumeMedisId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResepTemplateDetails");

            migrationBuilder.DropTable(
                name: "ResumePulangs");

            migrationBuilder.AddColumn<string>(
                name: "InteraturObat",
                schema: "public",
                table: "MstResepTemplate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObatId",
                schema: "public",
                table: "MstResepTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Qty",
                schema: "public",
                table: "MstResepTemplate",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signa",
                schema: "public",
                table: "MstResepTemplate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignaTambahan",
                schema: "public",
                table: "MstResepTemplate",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstResepTemplate_ObatId",
                schema: "public",
                table: "MstResepTemplate",
                column: "ObatId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResepTemplate_MstObat_ObatId",
                schema: "public",
                table: "MstResepTemplate",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId");
        }
    }
}
