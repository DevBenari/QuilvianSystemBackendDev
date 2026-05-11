using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newARcoa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fin_DetailDokumenReceived",
                schema: "public",
                columns: table => new
                {
                    DetailDokReceivedId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoBilling = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SuratPengantar = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Kwitansi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RekapitulasiTagihan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Invoice = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TandaTerima = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TglTerima = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglKirim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglTagihan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalPiutang = table.Column<decimal>(type: "numeric", nullable: true),
                    TglJaatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsTerbayar = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetailDokumenReceived", x => x.DetailDokReceivedId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_DetailInvoiceReceived",
                schema: "public",
                columns: table => new
                {
                    DetailInvoicePaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasiemId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NamaPasien = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NoBilling = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TglTerima = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglKirim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglTagihan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalPiutang = table.Column<decimal>(type: "numeric", nullable: true),
                    TglJaatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsTerbayar = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetailInvoiceReceived", x => x.DetailInvoicePaymentId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_DetailReceivedPayment",
                schema: "public",
                columns: table => new
                {
                    DetailReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoInvoice = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalInvoice = table.Column<decimal>(type: "numeric", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCanceled = table.Column<bool>(type: "boolean", nullable: true),
                    COADiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaCOADiskon = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PersenCOADiskon = table.Column<decimal>(type: "numeric", nullable: true),
                    COATambahanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaCoaTambahan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NominalTambahan = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetailReceivedPayment", x => x.DetailReceivedPaymentId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_MasterCoa",
                schema: "public",
                columns: table => new
                {
                    COAId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupCOAId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaCOA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KodeCOA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPostable = table.Column<bool>(type: "boolean", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: true),
                    IsPLACC = table.Column<bool>(type: "boolean", nullable: true),
                    NomalBalance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_MasterCoa", x => x.COAId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_MasterGrup",
                schema: "public",
                columns: table => new
                {
                    GrupCOAId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipeAkunCOAId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaGrupCOA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KodeGrupCOA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_MasterGrup", x => x.GrupCOAId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_ReceivedPayment",
                schema: "public",
                columns: table => new
                {
                    ReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalReceived = table.Column<decimal>(type: "numeric", nullable: true),
                    TglPembayaran = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SisaPembayaran = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalTagihanPasien = table.Column<decimal>(type: "numeric", nullable: true),
                    PembayaranKe = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_ReceivedPayment", x => x.ReceivedPaymentId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_TipeAkun",
                schema: "public",
                columns: table => new
                {
                    TipeAkunCOAId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaTipeAkunCOA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KodeTipeAkunCOA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_TipeAkun", x => x.TipeAkunCOAId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_BankAccount",
                schema: "public",
                columns: table => new
                {
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankShortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NoAccount = table.Column<decimal>(type: "numeric", nullable: true),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_BankAccount", x => x.BankAccountId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MasterBank",
                schema: "public",
                columns: table => new
                {
                    BankId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankShortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Hrd_MasterBank", x => x.BankId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_DetailDokumenReceived",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_DetailInvoiceReceived",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_DetailReceivedPayment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_MasterCoa",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_MasterGrup",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_ReceivedPayment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_TipeAkun",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_BankAccount",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MasterBank",
                schema: "public");
        }
    }
}
