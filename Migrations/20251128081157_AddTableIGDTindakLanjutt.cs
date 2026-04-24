using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableIGDTindakLanjutt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TTDId",
                table: "CttPemberianObats");

            migrationBuilder.RenameColumn(
                name: "TTId",
                table: "ResumePulangDetails",
                newName: "PerawatId");

            migrationBuilder.AddColumn<string>(
                name: "TTDPerawatPath",
                table: "ResumePulangDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TTDPerawatPath",
                table: "CttPemberianObats",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IGDTindakLanjuts",
                columns: table => new
                {
                    TindakLanjutIgdId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaktuPindah = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TindakanLanjutan = table.Column<string>(type: "text", nullable: true),
                    StatusPasien = table.Column<string>(type: "text", nullable: true),
                    WaktuStatus = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KontrolKe = table.Column<string>(type: "text", nullable: true),
                    WaktuKontrol = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Transportasi = table.Column<string>(type: "text", nullable: true),
                    AlasanMenolakDirawat = table.Column<string>(type: "text", nullable: true),
                    RsRujukan = table.Column<string>(type: "text", nullable: true),
                    AlasanDirujuk = table.Column<string>(type: "text", nullable: true),
                    TingkatKesadaran = table.Column<string>(type: "text", nullable: true),
                    Eyes = table.Column<string>(type: "text", nullable: true),
                    Motorik = table.Column<string>(type: "text", nullable: true),
                    Verbal = table.Column<string>(type: "text", nullable: true),
                    Pupil = table.Column<string>(type: "text", nullable: true),
                    Reaksi = table.Column<string>(type: "text", nullable: true),
                    Suhu = table.Column<decimal>(type: "numeric", nullable: true),
                    TD = table.Column<string>(type: "text", nullable: true),
                    Nadi = table.Column<decimal>(type: "numeric", nullable: true),
                    RR = table.Column<decimal>(type: "numeric", nullable: true),
                    SPO2 = table.Column<decimal>(type: "numeric", nullable: true),
                    HasilLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasilCTScanId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasilEKGId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasilRontgenId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasilUSGId = table.Column<Guid>(type: "uuid", nullable: true),
                    LembarLab = table.Column<decimal>(type: "numeric", nullable: true),
                    LembarCTScan = table.Column<decimal>(type: "numeric", nullable: true),
                    LembarEKG = table.Column<decimal>(type: "numeric", nullable: true),
                    LembarRontgen = table.Column<decimal>(type: "numeric", nullable: true),
                    LembarUSG = table.Column<decimal>(type: "numeric", nullable: true),
                    PerawatIgdId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatKamarId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_IGDTindakLanjuts", x => x.TindakLanjutIgdId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TTDPerawatPath",
                table: "ResumePulangDetails");

            migrationBuilder.DropColumn(
                name: "TTDPerawatPath",
                table: "CttPemberianObats");

            migrationBuilder.RenameColumn(
                name: "PerawatId",
                table: "ResumePulangDetails",
                newName: "TTId");

            migrationBuilder.AddColumn<Guid>(
                name: "TTDId",
                table: "CttPemberianObats",
                type: "uuid",
                nullable: true);
        }
    }
}
