using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addTableDokAyatSilang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIN_ARSettlement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FIN_ARSettlementDetail",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "Fin_AyatSilang",
                schema: "public",
                columns: table => new
                {
                    AyatSilangId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoReferensi = table.Column<string>(type: "text", nullable: false),
                    NoAyatSilang = table.Column<string>(type: "text", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPembayaran = table.Column<decimal>(type: "numeric", nullable: false),
                    TglPembayaran = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserProcess = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSudahTerpakai = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Fin_AyatSilang", x => x.AyatSilangId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_DokAyatSilang",
                schema: "public",
                columns: table => new
                {
                    DokAyatSilangId = table.Column<Guid>(type: "uuid", nullable: false),
                    AyatSilangId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaDokumen = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    TglPenyimpanan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Fin_DokAyatSilang", x => x.DokAyatSilangId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_TransaksiAyatSilang",
                schema: "public",
                columns: table => new
                {
                    TransAyatSilangId = table.Column<Guid>(type: "uuid", nullable: false),
                    AyatSilangId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglTransaksiMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SaldoKredit = table.Column<decimal>(type: "numeric", nullable: false),
                    TglTransaksiKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SaldoDebet = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Fin_TransaksiAyatSilang", x => x.TransAyatSilangId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_AyatSilang",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_DokAyatSilang",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_TransaksiAyatSilang",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "FIN_ARSettlement",
                schema: "public",
                columns: table => new
                {
                    SettlementARId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeginingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_ARSettlement", x => x.SettlementARId);
                });

            migrationBuilder.CreateTable(
                name: "FIN_ARSettlementDetail",
                schema: "public",
                columns: table => new
                {
                    DetailSettlementARId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsCanceled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    JumlahUang = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
                    NoBill = table.Column<string>(type: "text", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    NoRegistrasi = table.Column<string>(type: "text", nullable: false),
                    PembayaranKe = table.Column<int>(type: "integer", nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric", nullable: false),
                    SettlementARId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglTransaksi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipeSettlement = table.Column<string>(type: "text", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIN_ARSettlementDetail", x => x.DetailSettlementARId);
                });
        }
    }
}
