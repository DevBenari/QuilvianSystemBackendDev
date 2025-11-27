using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableIGDNosokomial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<Guid>>(
                name: "TindakanPerawatId",
                table: "TindakanHarians",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "InfeksiADPs",
                columns: table => new
                {
                    InfeksiADPId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsInfusVenaPerifer = table.Column<bool>(type: "boolean", nullable: true),
                    IsCVP = table.Column<bool>(type: "boolean", nullable: true),
                    IsKateterDarah = table.Column<bool>(type: "boolean", nullable: true),
                    HasilLabLeokosit = table.Column<string>(type: "text", nullable: true),
                    HasilLabHB = table.Column<string>(type: "text", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_InfeksiADPs", x => x.InfeksiADPId);
                });

            migrationBuilder.CreateTable(
                name: "InfeksiDetails",
                columns: table => new
                {
                    DetailInfeksiId = table.Column<Guid>(type: "uuid", nullable: false),
                    InfeksiId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    HariKe = table.Column<int>(type: "integer", nullable: true),
                    LokasiReaksi = table.Column<string>(type: "text", nullable: true),
                    TglMulaiReaksi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAkhirReaksi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Nyeri = table.Column<string>(type: "text", nullable: true),
                    Merah = table.Column<string>(type: "text", nullable: true),
                    Bengkak = table.Column<string>(type: "text", nullable: true),
                    PUS = table.Column<string>(type: "text", nullable: true),
                    Menggigil = table.Column<string>(type: "text", nullable: true),
                    IsDemam = table.Column<bool>(type: "boolean", nullable: true),
                    Drainase = table.Column<string>(type: "text", nullable: true),
                    Perforasi = table.Column<string>(type: "text", nullable: true),
                    Fistula = table.Column<string>(type: "text", nullable: true),
                    NyeriSupraPublik = table.Column<string>(type: "text", nullable: true),
                    NyeriSaatBerkemih = table.Column<string>(type: "text", nullable: true),
                    PasangDCKe = table.Column<string>(type: "text", nullable: true),
                    AnyangAnyangan = table.Column<string>(type: "text", nullable: true),
                    Gatal = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_InfeksiDetails", x => x.DetailInfeksiId);
                });

            migrationBuilder.CreateTable(
                name: "InfeksiLOs",
                columns: table => new
                {
                    InfeksiLOId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDarurat = table.Column<bool>(type: "boolean", nullable: true),
                    IsAnastesiUmum = table.Column<bool>(type: "boolean", nullable: true),
                    RondeKe = table.Column<string>(type: "text", nullable: true),
                    IsTrauma = table.Column<bool>(type: "boolean", nullable: true),
                    IsProsedurMultiple = table.Column<bool>(type: "boolean", nullable: true),
                    ASAScore = table.Column<decimal>(type: "numeric", nullable: true),
                    IsHbsag = table.Column<bool>(type: "boolean", nullable: true),
                    IsAntiHCV = table.Column<bool>(type: "boolean", nullable: true),
                    HasilLabLeukosit = table.Column<string>(type: "text", nullable: true),
                    HasilLabHB = table.Column<string>(type: "text", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_InfeksiLOs", x => x.InfeksiLOId);
                });

            migrationBuilder.CreateTable(
                name: "InfeksiSKs",
                columns: table => new
                {
                    InfeksiSKId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    KateterUrin = table.Column<string>(type: "text", nullable: true),
                    TglLeukositUrin1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglLeukositUrin2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglBiakanUrin1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglBiakanUrin2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasilBiakanUrin1 = table.Column<string>(type: "text", nullable: true),
                    HasilBiakanUrin2 = table.Column<string>(type: "text", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_InfeksiSKs", x => x.InfeksiSKId);
                });

            migrationBuilder.CreateTable(
                name: "InfeksiTDs",
                columns: table => new
                {
                    InfeksiTransfusiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglTransfusi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JenisTransfusi = table.Column<string>(type: "text", nullable: true),
                    Jumlah = table.Column<decimal>(type: "numeric", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_InfeksiTDs", x => x.InfeksiTransfusiId);
                });

            migrationBuilder.CreateTable(
                name: "KulturDarahs",
                columns: table => new
                {
                    KulturDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    InfeksiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglKulturDarah = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasilKulturDarah = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KulturDarahs", x => x.KulturDarahId);
                });

            migrationBuilder.CreateTable(
                name: "Nosokomials",
                columns: table => new
                {
                    NosokomialId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TB = table.Column<decimal>(type: "numeric", nullable: true),
                    BB = table.Column<decimal>(type: "numeric", nullable: true),
                    CaraMasukRS = table.Column<string>(type: "text", nullable: true),
                    TglMasukRs = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglKeluarRs = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DokterId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId2 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId3 = table.Column<Guid>(type: "uuid", nullable: true),
                    IPCLN1 = table.Column<Guid>(type: "uuid", nullable: true),
                    IPCLN2 = table.Column<Guid>(type: "uuid", nullable: true),
                    IPCLN3 = table.Column<Guid>(type: "uuid", nullable: true),
                    KondisiKeluar = table.Column<string>(type: "text", nullable: true),
                    DiagnosaAwal = table.Column<string>(type: "text", nullable: true),
                    DiagnosaAkhir = table.Column<string>(type: "text", nullable: true),
                    TTDKepalaRuangan = table.Column<string>(type: "text", nullable: true),
                    NamaKepalaRuangan = table.Column<string>(type: "text", nullable: true),
                    TTDPerawat = table.Column<string>(type: "text", nullable: true),
                    NamaPerawat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Nosokomials", x => x.NosokomialId);
                });

            migrationBuilder.CreateTable(
                name: "PindahRuangans",
                columns: table => new
                {
                    PindahRuanganId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglAwal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAkhir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PindahRuangans", x => x.PindahRuanganId);
                });

            migrationBuilder.CreateTable(
                name: "Pneumonias",
                columns: table => new
                {
                    PneumoniaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsFotoThorax = table.Column<bool>(type: "boolean", nullable: true),
                    IsHAP = table.Column<bool>(type: "boolean", nullable: true),
                    HasilFotoThorax = table.Column<string>(type: "text", nullable: true),
                    DokterHAPId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsVAP = table.Column<bool>(type: "boolean", nullable: true),
                    DokterVAPId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsVentilatorTerpasang = table.Column<bool>(type: "boolean", nullable: true),
                    TglAwalVT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAkhirVT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HariKe = table.Column<int>(type: "integer", nullable: true),
                    HasilThoraxSebelumVT = table.Column<string>(type: "text", nullable: true),
                    HasilThoraxSesudahVT = table.Column<string>(type: "text", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Pneumonias", x => x.PneumoniaId);
                });

            migrationBuilder.CreateTable(
                name: "UlkusDebituss",
                columns: table => new
                {
                    UlkusDekubitusId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglAwalTirahBaring = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAkhirTirahBaring = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglDekubitus = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AsalDekubitus = table.Column<string>(type: "text", nullable: true),
                    NamaTempatDekubitus = table.Column<string>(type: "text", nullable: true),
                    IndicatorScoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPencatatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LokasiUlkusDekubitus = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UlkusDebituss", x => x.UlkusDekubitusId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InfeksiADPs");

            migrationBuilder.DropTable(
                name: "InfeksiDetails");

            migrationBuilder.DropTable(
                name: "InfeksiLOs");

            migrationBuilder.DropTable(
                name: "InfeksiSKs");

            migrationBuilder.DropTable(
                name: "InfeksiTDs");

            migrationBuilder.DropTable(
                name: "KulturDarahs");

            migrationBuilder.DropTable(
                name: "Nosokomials");

            migrationBuilder.DropTable(
                name: "PindahRuangans");

            migrationBuilder.DropTable(
                name: "Pneumonias");

            migrationBuilder.DropTable(
                name: "UlkusDebituss");

            migrationBuilder.AlterColumn<Guid>(
                name: "TindakanPerawatId",
                table: "TindakanHarians",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(List<Guid>),
                oldType: "uuid[]",
                oldNullable: true);
        }
    }
}
