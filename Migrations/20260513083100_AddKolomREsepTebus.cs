using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomREsepTebus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstalasiUnitId",
                schema: "public",
                table: "ResepTebusDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObatUnitId",
                schema: "public",
                table: "ResepTebusDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsalFaskes",
                schema: "public",
                table: "ResepTebus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstalasiUnitId",
                schema: "public",
                table: "ResepTebus",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisLayanan",
                schema: "public",
                table: "ResepTebus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoHpPenebus",
                schema: "public",
                table: "ResepTebus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoResepLuar",
                schema: "public",
                table: "ResepTebus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PetugasFarmasiId",
                schema: "public",
                table: "ResepTebus",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalLunas",
                schema: "public",
                table: "ResepTebus",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHargaResep",
                schema: "public",
                table: "ResepTebus",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisAR",
                schema: "public",
                table: "FIN_ARHeader",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tipe_Kunjungan",
                schema: "public",
                table: "FIN_ARHeader",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FIN_ARSettlement",
                schema: "public",
                columns: table => new
                {
                    SettlementARId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    BeginingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    EndingBalance = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ARSettlement", x => x.SettlementARId);
                });

            migrationBuilder.CreateTable(
                name: "FIN_ARSettlementDetail",
                schema: "public",
                columns: table => new
                {
                    DetailSettlementARId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementARId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoRegistrasi = table.Column<string>(type: "text", nullable: false),
                    NoBill = table.Column<string>(type: "text", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    TglTransaksi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JumlahUang = table.Column<decimal>(type: "numeric", nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric", nullable: false),
                    PembayaranKe = table.Column<int>(type: "integer", nullable: false),
                    IsCanceled = table.Column<bool>(type: "boolean", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    TipeSettlement = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ARSettlementDetail", x => x.DetailSettlementARId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIN_ARSettlement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FIN_ARSettlementDetail",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "InstalasiUnitId",
                schema: "public",
                table: "ResepTebusDetail");

            migrationBuilder.DropColumn(
                name: "ObatUnitId",
                schema: "public",
                table: "ResepTebusDetail");

            migrationBuilder.DropColumn(
                name: "AsalFaskes",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "InstalasiUnitId",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "JenisLayanan",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "NoHpPenebus",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "NoResepLuar",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "PetugasFarmasiId",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "TanggalLunas",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "TotalHargaResep",
                schema: "public",
                table: "ResepTebus");

            migrationBuilder.DropColumn(
                name: "JenisAR",
                schema: "public",
                table: "FIN_ARHeader");

            migrationBuilder.DropColumn(
                name: "Tipe_Kunjungan",
                schema: "public",
                table: "FIN_ARHeader");
        }
    }
}
