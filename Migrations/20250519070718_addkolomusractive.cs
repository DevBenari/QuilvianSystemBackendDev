using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addkolomusractive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatasMaxKlaimPerKunjungan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "BatasMaxKlaimPerTahun",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "PersentasiBiayaPertanggungan",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.RenameColumn(
                name: "TanggalRegist",
                schema: "public",
                table: "MstAsuransi",
                newName: "noVerificationAdmin");

            migrationBuilder.RenameColumn(
                name: "NoTelepon",
                schema: "public",
                table: "MstAsuransi",
                newName: "noPic");

            migrationBuilder.RenameColumn(
                name: "NoRekRumahSakit",
                schema: "public",
                table: "MstAsuransi",
                newName: "namaPIC");

            migrationBuilder.RenameColumn(
                name: "NamaBank",
                schema: "public",
                table: "MstAsuransi",
                newName: "Keterangan");

            migrationBuilder.RenameColumn(
                name: "KategoriAsuransi",
                schema: "public",
                table: "MstAsuransi",
                newName: "Alamat");

            migrationBuilder.AddColumn<Guid>(
                name: "UserActiveId",
                schema: "public",
                table: "MstDokter",
                type: "uuid",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserActiveId",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.RenameColumn(
                name: "noVerificationAdmin",
                schema: "public",
                table: "MstAsuransi",
                newName: "TanggalRegist");

            migrationBuilder.RenameColumn(
                name: "noPic",
                schema: "public",
                table: "MstAsuransi",
                newName: "NoTelepon");

            migrationBuilder.RenameColumn(
                name: "namaPIC",
                schema: "public",
                table: "MstAsuransi",
                newName: "NoRekRumahSakit");

            migrationBuilder.RenameColumn(
                name: "Keterangan",
                schema: "public",
                table: "MstAsuransi",
                newName: "NamaBank");

            migrationBuilder.RenameColumn(
                name: "Alamat",
                schema: "public",
                table: "MstAsuransi",
                newName: "KategoriAsuransi");


            migrationBuilder.AddColumn<int>(
                name: "BatasMaxKlaimPerKunjungan",
                schema: "public",
                table: "MstAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatasMaxKlaimPerTahun",
                schema: "public",
                table: "MstAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PersentasiBiayaPertanggungan",
                schema: "public",
                table: "MstAsuransi",
                type: "numeric",
                nullable: true);
        }
    }
}
