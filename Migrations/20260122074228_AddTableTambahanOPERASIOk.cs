using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableTambahanOPERASIOk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatatanBedahLokals",
                columns: table => new
                {
                    CatBedahLokalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatBedahId = table.Column<Guid>(type: "uuid", nullable: true),
                    KomplikasiAkut = table.Column<string>(type: "text", nullable: true),
                    TemuanSaatOperasi = table.Column<string>(type: "text", nullable: true),
                    Pengawasan = table.Column<string>(type: "text", nullable: true),
                    Kontrol = table.Column<string>(type: "text", nullable: true),
                    Terapi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_CatatanBedahLokals", x => x.CatBedahLokalId);
                });

            migrationBuilder.CreateTable(
                name: "CatatanBedahs",
                columns: table => new
                {
                    CatBedahId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterOperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsistenDokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsistenAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    IcdPraOperasiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosaPraOperasi = table.Column<string>(type: "text", nullable: true),
                    IcdPostOperasiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosaPostOperasi = table.Column<string>(type: "text", nullable: true),
                    JenisOperasi = table.Column<string>(type: "text", nullable: true),
                    UrgensiOperasi = table.Column<string>(type: "text", nullable: true),
                    MacamOperasi = table.Column<string>(type: "text", nullable: true),
                    TanggalOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Jumlah = table.Column<decimal>(type: "numeric", nullable: true),
                    WaktuMulaiOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuSelesaiOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuTambahan = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    LamaOperasi = table.Column<TimeSpan>(type: "interval", nullable: true),
                    JumlahPendarahan = table.Column<decimal>(type: "numeric", nullable: true),
                    IsJaringan = table.Column<bool>(type: "boolean", nullable: true),
                    JenisJaringan = table.Column<string>(type: "text", nullable: true),
                    IsPA = table.Column<bool>(type: "boolean", nullable: true),
                    Komplikasi = table.Column<string>(type: "text", nullable: true),
                    CatatanSaatOperasi = table.Column<string>(type: "text", nullable: true),
                    PathTTDDokterOperator = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatatanBedahs", x => x.CatBedahId);
                });

            migrationBuilder.CreateTable(
                name: "catatanPemulihanDetails",
                columns: table => new
                {
                    DetailCatPemulihanId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaktuPengawasan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PengawasanTDPostOP = table.Column<string>(type: "text", nullable: true),
                    BilaSistole = table.Column<decimal>(type: "numeric", nullable: true),
                    PengawasanTerapi = table.Column<string>(type: "text", nullable: true),
                    IntruksiKhusus = table.Column<string>(type: "text", nullable: true),
                    IntruksiSedasi = table.Column<string>(type: "text", nullable: true),
                    NilaiNumeric = table.Column<decimal>(type: "numeric", nullable: true),
                    NilaiRespirasi = table.Column<decimal>(type: "numeric", nullable: true),
                    NilaiSirkulasi = table.Column<decimal>(type: "numeric", nullable: true),
                    NilaiKesadaran = table.Column<decimal>(type: "numeric", nullable: true),
                    NilaiWarnaKulit = table.Column<decimal>(type: "numeric", nullable: true),
                    JumlahScoreAldrete = table.Column<decimal>(type: "numeric", nullable: true),
                    IsAldreteDewasa = table.Column<bool>(type: "boolean", nullable: true),
                    BromageScore = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_catatanPemulihanDetails", x => x.DetailCatPemulihanId);
                });

            migrationBuilder.CreateTable(
                name: "CatatanPemulihans",
                columns: table => new
                {
                    CatatanPemulihanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterOperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaktuMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InfusTransfusi = table.Column<string>(type: "text", nullable: true),
                    JumlahUrine = table.Column<decimal>(type: "numeric", nullable: true),
                    Komplikasi = table.Column<string>(type: "text", nullable: true),
                    Penatalaksanaan = table.Column<string>(type: "text", nullable: true),
                    InfusSedasi = table.Column<string>(type: "text", nullable: true),
                    Antibiotika = table.Column<string>(type: "text", nullable: true),
                    Analgetik = table.Column<string>(type: "text", nullable: true),
                    AntiMuntah = table.Column<string>(type: "text", nullable: true),
                    Minum = table.Column<string>(type: "text", nullable: true),
                    PosisiPasien = table.Column<string>(type: "text", nullable: true),
                    Dipindahkan = table.Column<string>(type: "text", nullable: true),
                    WaktuKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PathDokterOperator = table.Column<string>(type: "text", nullable: true),
                    PathPerawat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_CatatanPemulihans", x => x.CatatanPemulihanId);
                });

            migrationBuilder.CreateTable(
                name: "LaporanBedahs",
                columns: table => new
                {
                    LaporanBedahId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailTindakan = table.Column<string>(type: "text", nullable: true),
                    DokterOperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterAsistenId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsistenAnestesiId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisAnestesi = table.Column<string>(type: "text", nullable: true),
                    DiagnosaPraOp = table.Column<string>(type: "text", nullable: true),
                    DiagnosaPostOp = table.Column<string>(type: "text", nullable: true),
                    JaringanEksisiInsisi = table.Column<string>(type: "text", nullable: true),
                    TipeUrgensi = table.Column<string>(type: "text", nullable: true),
                    IsPemeriksaanPA = table.Column<bool>(type: "boolean", nullable: true),
                    TanggalOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuMulaiOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuSelesaiOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurasiOperasi = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LaporanOperasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LaporanBedahs", x => x.LaporanBedahId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatatanBedahLokals");

            migrationBuilder.DropTable(
                name: "CatatanBedahs");

            migrationBuilder.DropTable(
                name: "catatanPemulihanDetails");

            migrationBuilder.DropTable(
                name: "CatatanPemulihans");

            migrationBuilder.DropTable(
                name: "LaporanBedahs");
        }
    }
}
