using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePengkajian : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PengkajianEliminasis",
                columns: table => new
                {
                    EliminasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PengkajianPerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    MasalahPerkemihan = table.Column<string>(type: "text", nullable: true),
                    MasalahDefekasi = table.Column<string>(type: "text", nullable: true),
                    WarnaBAK = table.Column<string>(type: "text", nullable: true),
                    AlatBantuEliminasi = table.Column<string>(type: "text", nullable: true),
                    JenisKateter = table.Column<string>(type: "text", nullable: true),
                    UkuranKateter = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PengkajianEliminasis", x => x.EliminasiId);
                });

            migrationBuilder.CreateTable(
                name: "PengkajianKetergantungans",
                columns: table => new
                {
                    KetergantunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PengkajianPerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Mobilitas = table.Column<string>(type: "text", nullable: true),
                    Personal = table.Column<string>(type: "text", nullable: true),
                    Toileting = table.Column<string>(type: "text", nullable: true),
                    MakanMinum = table.Column<string>(type: "text", nullable: true),
                    Kesadaran = table.Column<string>(type: "text", nullable: true),
                    ObservasiTTV = table.Column<string>(type: "text", nullable: true),
                    Respirasi = table.Column<string>(type: "text", nullable: true),
                    Pengobatan = table.Column<string>(type: "text", nullable: true),
                    IsLaporDPJP = table.Column<bool>(type: "boolean", nullable: true),
                    AlatBantuADL = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PengkajianKetergantungans", x => x.KetergantunganId);
                });

            migrationBuilder.CreateTable(
                name: "PengkajianKulits",
                columns: table => new
                {
                    IntegritasKulitId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PengkajianPerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTerganggu = table.Column<bool>(type: "boolean", nullable: true),
                    SkalaDekubitus = table.Column<decimal>(type: "numeric", nullable: true),
                    KondisiKulit = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_PengkajianKulits", x => x.IntegritasKulitId);
                });

            migrationBuilder.CreateTable(
                name: "PengkajianPerawats",
                columns: table => new
                {
                    PengkajianPerawatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendaftaranPasienBaruId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    SumberData = table.Column<string>(type: "text", nullable: true),
                    HubunganDenganPasien = table.Column<string>(type: "text", nullable: true),
                    TglMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglPengkajianPerawat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MasalahPsikologi = table.Column<string>(type: "text", nullable: true),
                    IsHubunganSosial = table.Column<bool>(type: "boolean", nullable: true),
                    TempatTinggal = table.Column<string>(type: "text", nullable: true),
                    GangguanFungsional = table.Column<string>(type: "text", nullable: true),
                    NilaiKepercayaan = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PengkajianPerawats", x => x.PengkajianPerawatId);
                });

            migrationBuilder.CreateTable(
                name: "PengkajianPernapasans",
                columns: table => new
                {
                    PernapasanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PengkajianPerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsSulitBernapas = table.Column<bool>(type: "boolean", nullable: true),
                    PemakaianO2 = table.Column<decimal>(type: "numeric", nullable: true),
                    AlatO2 = table.Column<string>(type: "text", nullable: true),
                    IsBatukProduktive = table.Column<bool>(type: "boolean", nullable: true),
                    PolaPernapasan = table.Column<string>(type: "text", nullable: true),
                    MasalahPernapasan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PengkajianPernapasans", x => x.PernapasanId);
                });

            migrationBuilder.CreateTable(
                name: "SkriningNutrisis",
                columns: table => new
                {
                    SkriningNutrisiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsIMT85 = table.Column<bool>(type: "boolean", nullable: true),
                    IsWeightLoss3mo = table.Column<bool>(type: "boolean", nullable: true),
                    IsLowFoodIntake1wk = table.Column<bool>(type: "boolean", nullable: true),
                    IsPasienKurus = table.Column<bool>(type: "boolean", nullable: true),
                    IsWeightLoss1mo = table.Column<bool>(type: "boolean", nullable: true),
                    IsWeightStable3mo = table.Column<bool>(type: "boolean", nullable: true),
                    IsDiareGt5 = table.Column<bool>(type: "boolean", nullable: true),
                    IsVomitgt5 = table.Column<bool>(type: "boolean", nullable: true),
                    IsNafsuMakanMenurun = table.Column<bool>(type: "boolean", nullable: true),
                    GangguanMetabolisme = table.Column<string>(type: "text", nullable: true),
                    IsWeightLossOrWeightGain = table.Column<bool>(type: "boolean", nullable: true),
                    IsHBHCTBermasalah = table.Column<bool>(type: "boolean", nullable: true),
                    IsPenyakitBerat = table.Column<bool>(type: "boolean", nullable: true),
                    IsMalnutrisi = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_SkriningNutrisis", x => x.SkriningNutrisiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PengkajianEliminasis");

            migrationBuilder.DropTable(
                name: "PengkajianKetergantungans");

            migrationBuilder.DropTable(
                name: "PengkajianKulits");

            migrationBuilder.DropTable(
                name: "PengkajianPerawats");

            migrationBuilder.DropTable(
                name: "PengkajianPernapasans");

            migrationBuilder.DropTable(
                name: "SkriningNutrisis");
        }
    }
}
