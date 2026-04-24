using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtablekasir : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLunas",
                schema: "public",
                table: "MstResep",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BiayaAdministrasi",
                schema: "public",
                columns: table => new
                {
                    BiayaAdministrasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    BiayaAdministrasiKode = table.Column<string>(type: "text", nullable: true),
                    NamaBiayaAdministrasi = table.Column<string>(type: "text", nullable: true),
                    NominalBiayaAdministrasi = table.Column<decimal>(type: "numeric", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiayaAdministrasi", x => x.BiayaAdministrasiId);
                });

            migrationBuilder.CreateTable(
                name: "Diskon",
                schema: "public",
                columns: table => new
                {
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaDiskon = table.Column<string>(type: "text", nullable: true),
                    TglBerlaku = table.Column<DateOnly>(type: "date", nullable: true),
                    TglBerakhir = table.Column<DateOnly>(type: "date", nullable: true),
                    IsAsuransi = table.Column<bool>(type: "boolean", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersenDiskon = table.Column<decimal>(type: "numeric", nullable: true),
                    NominalDiskon = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diskon", x => x.DiskonId);
                });

            migrationBuilder.CreateTable(
                name: "MainKasir",
                schema: "public",
                columns: table => new
                {
                    KasirId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    BiayaAdministrasiKode = table.Column<string>(type: "text", nullable: true),
                    MetodePembayaranId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaMetode = table.Column<string>(type: "text", nullable: true),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    NominalPembayaran = table.Column<decimal>(type: "numeric", nullable: true),
                    StatusPembayaran = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglPembayaran = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MainKasir", x => x.KasirId);
                });

            migrationBuilder.CreateTable(
                name: "MetodePembayaran",
                schema: "public",
                columns: table => new
                {
                    MetodePembayaranId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaMetode = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodePembayaran", x => x.MetodePembayaranId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BiayaAdministrasi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Diskon",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MainKasir",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MetodePembayaran",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "IsLunas",
                schema: "public",
                table: "MstResep");
        }
    }
}
