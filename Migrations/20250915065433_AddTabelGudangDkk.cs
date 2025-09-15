using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTabelGudangDkk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaraPemakaian",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimasiPemberian",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglStopPemakaian",
                schema: "public",
                table: "MstResepDetail",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CttPemberianObats",
                columns: table => new
                {
                    CttPemberianObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPemberian = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuPemberian = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    StatusPemberian = table.Column<string>(type: "text", nullable: true),
                    CaraPemberianObat = table.Column<string>(type: "text", nullable: true),
                    UserActiveIdPerawat = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CttPemberianObats", x => x.CttPemberianObatId);
                });

            migrationBuilder.CreateTable(
                name: "DetailPermintaanUnits",
                columns: table => new
                {
                    DetailPermintaanUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermintaanUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtyPermintaan = table.Column<decimal>(type: "numeric", nullable: true),
                    SatuanItem = table.Column<string>(type: "text", nullable: true),
                    KategoriItem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DetailPermintaanUnits", x => x.DetailPermintaanUnitId);
                });

            migrationBuilder.CreateTable(
                name: "GudangUnits",
                columns: table => new
                {
                    GudangUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    GudangId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockGudangUnit = table.Column<decimal>(type: "numeric", nullable: true),
                    MinStockGudangUnit = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxStockGudangUnit = table.Column<decimal>(type: "numeric", nullable: true),
                    StockPenyanggaGudangUnit = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_GudangUnits", x => x.GudangUnitId);
                });

            migrationBuilder.CreateTable(
                name: "LogRacikPenerimaans",
                columns: table => new
                {
                    LogPeracikanPenerimaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserActiveFarmasiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaFarmasi = table.Column<string>(type: "text", nullable: true),
                    TglPeracikan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserActivePerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPerawat = table.Column<string>(type: "text", nullable: true),
                    TglPengambilanObat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_LogRacikPenerimaans", x => x.LogPeracikanPenerimaanId);
                });

            migrationBuilder.CreateTable(
                name: "MstGudang",
                schema: "public",
                columns: table => new
                {
                    GudangId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaGudang = table.Column<string>(type: "text", nullable: true),
                    Lokasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstGudang", x => x.GudangId);
                });

            migrationBuilder.CreateTable(
                name: "PermintaanUnits",
                columns: table => new
                {
                    PermintaanUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisPermintaan = table.Column<string>(type: "text", nullable: true),
                    TglPembuatanPermintaan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusPermintaan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PermintaanUnits", x => x.PermintaanUnitId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CttPemberianObats");

            migrationBuilder.DropTable(
                name: "DetailPermintaanUnits");

            migrationBuilder.DropTable(
                name: "GudangUnits");

            migrationBuilder.DropTable(
                name: "LogRacikPenerimaans");

            migrationBuilder.DropTable(
                name: "MstGudang",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PermintaanUnits");

            migrationBuilder.DropColumn(
                name: "CaraPemakaian",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "EstimasiPemberian",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "TglStopPemakaian",
                schema: "public",
                table: "MstResepDetail");
        }
    }
}
