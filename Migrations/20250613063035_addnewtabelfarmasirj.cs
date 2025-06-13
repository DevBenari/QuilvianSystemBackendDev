using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addnewtabelfarmasirj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AntrianRegistrasi",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AntrianResep",
                schema: "public",
                table: "MstResep",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                schema: "public",
                table: "MstResep",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaAsuransi",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaDokter",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPasien",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPoliklinik",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PasienId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PoliklinikId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPembuatanResep",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusPengambilan",
                schema: "public",
                table: "MstResep",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TanggalPembuatanResep",
                schema: "public",
                table: "MstResep",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "MstDetailResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaObat",
                schema: "public",
                table: "MstDetailResep",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisObat",
                schema: "public",
                table: "MstDetailResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaAsuransi",
                schema: "public",
                table: "MstDetailResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusCoverObat",
                schema: "public",
                table: "MstDetailResep",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FarmasiRJ",
                schema: "public",
                columns: table => new
                {
                    FarmasiRJId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    KonversiSatuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtySatuan = table.Column<decimal>(type: "numeric", nullable: true),
                    QtyKonversi = table.Column<decimal>(type: "numeric", nullable: true),
                    BatchNumber = table.Column<string>(type: "text", nullable: true),
                    RackLocation = table.Column<string>(type: "text", nullable: true),
                    TanggalKadaluarsa = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_FarmasiRJ", x => x.FarmasiRJId);
                });

            migrationBuilder.CreateTable(
                name: "MstKonversiSatuan",
                schema: "public",
                columns: table => new
                {
                    KonversiSatuanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    SatuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSatuan = table.Column<string>(type: "text", nullable: true),
                    TipeKonversi = table.Column<string>(type: "text", nullable: true),
                    NilaiKonversi = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstKonversiSatuan", x => x.KonversiSatuanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FarmasiRJ",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKonversiSatuan",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "AntrianRegistrasi",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "AntrianResep",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "AsuransiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "DokterId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "NamaAsuransi",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "NamaDokter",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "NamaPasien",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "NamaPoliklinik",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "PasienId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "PoliklinikId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "StatusPembuatanResep",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "StatusPengambilan",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "TanggalPembuatanResep",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "AsuransiId",
                schema: "public",
                table: "MstDetailResep");

            migrationBuilder.DropColumn(
                name: "HargaObat",
                schema: "public",
                table: "MstDetailResep");

            migrationBuilder.DropColumn(
                name: "JenisObat",
                schema: "public",
                table: "MstDetailResep");

            migrationBuilder.DropColumn(
                name: "NamaAsuransi",
                schema: "public",
                table: "MstDetailResep");

            migrationBuilder.DropColumn(
                name: "StatusCoverObat",
                schema: "public",
                table: "MstDetailResep");
        }
    }
}
