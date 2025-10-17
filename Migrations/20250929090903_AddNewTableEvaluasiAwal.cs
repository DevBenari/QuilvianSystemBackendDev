using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewTableEvaluasiAwal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TglKeluar",
                table: "ResumePulangs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglMasuk",
                table: "ResumePulangs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluasiAwalDetails",
                columns: table => new
                {
                    DetailEvaluasiAwalId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluasiAwalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglPenyimpanan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EvaluasiAwalDetails", x => x.DetailEvaluasiAwalId);
                });

            migrationBuilder.CreateTable(
                name: "EvaluasiAwals",
                columns: table => new
                {
                    EvaluasiAwalId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    KekuatanKemampuan = table.Column<string>(type: "text", nullable: true),
                    RiwayatKesehatan = table.Column<string>(type: "text", nullable: true),
                    KesehatanMental = table.Column<string>(type: "text", nullable: true),
                    TersedianyaDukungan = table.Column<string>(type: "text", nullable: true),
                    FinancialEvaluasiAwal = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    RiwayatObatAlternatif = table.Column<string>(type: "text", nullable: true),
                    RiwayatTrauma = table.Column<string>(type: "text", nullable: true),
                    HarapanHasil = table.Column<string>(type: "text", nullable: true),
                    AspekLegal = table.Column<string>(type: "text", nullable: true),
                    DischargePlanning = table.Column<string>(type: "text", nullable: true),
                    KebutuhanLain = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglEvaluasiAwal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EvaluasiAwals", x => x.EvaluasiAwalId);
                });

            migrationBuilder.CreateTable(
                name: "ResumePulangDetails",
                columns: table => new
                {
                    DetResumePulangId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResumePulangId = table.Column<Guid>(type: "uuid", nullable: true),
                    Is65th = table.Column<bool>(type: "boolean", nullable: true),
                    IsPercobaanBunuhDiri = table.Column<bool>(type: "boolean", nullable: true),
                    IsKorbanKriminal = table.Column<bool>(type: "boolean", nullable: true),
                    IsKeterbatasanMobilitas = table.Column<bool>(type: "boolean", nullable: true),
                    IsPerawatanLanjutan = table.Column<bool>(type: "boolean", nullable: true),
                    IsBantuanADL = table.Column<bool>(type: "boolean", nullable: true),
                    TransportasiPulang = table.Column<string>(type: "text", nullable: true),
                    IsPasienTinggalSendiri = table.Column<bool>(type: "boolean", nullable: true),
                    NamaWali = table.Column<string>(type: "text", nullable: true),
                    LetakKamarPasien = table.Column<string>(type: "text", nullable: true),
                    KondisiPenerangan = table.Column<string>(type: "text", nullable: true),
                    JarakKamarMandi = table.Column<string>(type: "text", nullable: true),
                    PerawatanYangDibantu = table.Column<string>(type: "text", nullable: true),
                    IsDibantuAlatMedis = table.Column<bool>(type: "boolean", nullable: true),
                    IsAlatBantu = table.Column<bool>(type: "boolean", nullable: true),
                    IsPerluBantuanKhusus = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglDetailResumePulang = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ResumePulangDetails", x => x.DetResumePulangId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluasiAwalDetails");

            migrationBuilder.DropTable(
                name: "EvaluasiAwals");

            migrationBuilder.DropTable(
                name: "ResumePulangDetails");

            migrationBuilder.DropColumn(
                name: "TglKeluar",
                table: "ResumePulangs");

            migrationBuilder.DropColumn(
                name: "TglMasuk",
                table: "ResumePulangs");
        }
    }
}
