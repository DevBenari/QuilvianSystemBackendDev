using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editparamkunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlasanKeluar",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "BedId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "DokterDPJId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "KamarId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "ReferensiKunjunganId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "StatusRanap",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "TglKeluarRanap",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "TglMasukRanap",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.RenameColumn(
                name: "SDKIDiagnosa",
                schema: "public",
                table: "SDKIEtiologi",
                newName: "SDKIDiagnosaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SDKIDiagnosaId",
                schema: "public",
                table: "SDKIEtiologi",
                newName: "SDKIDiagnosa");

            migrationBuilder.AddColumn<string>(
                name: "AlasanKeluar",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BedId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterDPJId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KamarId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferensiKunjunganId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusRanap",
                schema: "public",
                table: "MstKunjungan",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglKeluarRanap",
                schema: "public",
                table: "MstKunjungan",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglMasukRanap",
                schema: "public",
                table: "MstKunjungan",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
