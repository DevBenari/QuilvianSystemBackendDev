using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class updateasuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlamatCabang",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "AlamatPusat",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "BatasWaktuPembayaran",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "BiayaTidakDitanggung",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "DokumenKlaim",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "EmailPerwakilan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "IsPKS",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "JabatanPerwakilan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "Layanan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "MasaTunggu",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "MaxUsiaPasien",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "NamaBankCabang",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "NamaPerwakilan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "NoHotlineDarurat",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "NoTeleponPerwakilan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "ObatDitanggung",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "PenaltiTerlambatBayar",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "RSRekanan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "WaktuKlaim",
                schema: "public",
                table: "MstAsuransi");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlamatCabang",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlamatPusat",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BatasWaktuPembayaran",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BiayaTidakDitanggung",
                schema: "public",
                table: "MstAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DokumenKlaim",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailPerwakilan",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPKS",
                schema: "public",
                table: "MstAsuransi",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JabatanPerwakilan",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Layanan",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MasaTunggu",
                schema: "public",
                table: "MstAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsiaPasien",
                schema: "public",
                table: "MstAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaBankCabang",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPerwakilan",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoHotlineDarurat",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoTeleponPerwakilan",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObatDitanggung",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PenaltiTerlambatBayar",
                schema: "public",
                table: "MstAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RSRekanan",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WaktuKlaim",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
