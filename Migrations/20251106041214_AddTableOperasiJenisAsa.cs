using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableOperasiJenisAsa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BiayaPerpanjangan",
                table: "RuangBedahBookings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "JamPerpanjangan",
                table: "RuangBedahBookings",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KamarRecoveryId",
                table: "RuangBedahBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KelompokPasienAnastesi",
                table: "RuangBedahBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipeASAId",
                table: "RuangBedahBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipeAnastesiId",
                table: "RuangBedahBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeOperasi",
                table: "RuangBedahBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitAsal",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstAnastesiTipe",
                schema: "public",
                columns: table => new
                {
                    TipeAnastesiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaTipeAnastesi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstAnastesiTipe", x => x.TipeAnastesiId);
                });

            migrationBuilder.CreateTable(
                name: "MstASATipe",
                schema: "public",
                columns: table => new
                {
                    TipeASAId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaTipeASA = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstASATipe", x => x.TipeASAId);
                });

            migrationBuilder.CreateTable(
                name: "MstOperasiJenis",
                schema: "public",
                columns: table => new
                {
                    JenisOperasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaJenisOperasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstOperasiJenis", x => x.JenisOperasiId);
                });

            migrationBuilder.CreateTable(
                name: "MstOperasiTipe",
                schema: "public",
                columns: table => new
                {
                    TipeOperasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaTipeOperasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstOperasiTipe", x => x.TipeOperasiId);
                });

            migrationBuilder.CreateTable(
                name: "RuangBedahBookingDetails",
                columns: table => new
                {
                    DetailBookingBedahId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingRuanganBedahId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisOperasiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserActiveId = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
                    PersentaseTindakan = table.Column<decimal>(type: "numeric", nullable: true),
                    DiskonDokter = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_RuangBedahBookingDetails", x => x.DetailBookingBedahId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstAnastesiTipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstASATipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstOperasiJenis",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstOperasiTipe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RuangBedahBookingDetails");

            migrationBuilder.DropColumn(
                name: "BiayaPerpanjangan",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "JamPerpanjangan",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "KamarRecoveryId",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "KelompokPasienAnastesi",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "TipeASAId",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "TipeAnastesiId",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "TipeOperasi",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "UnitAsal",
                schema: "public",
                table: "MstTindakan");
        }
    }
}
