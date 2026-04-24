using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePemeriksaaanLabAsuransiDll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Diskon",
                schema: "public",
                table: "MstTindakanAsuransi",
                newName: "MarkupTotal");

            migrationBuilder.RenameColumn(
                name: "Diskon",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                newName: "MarkupTotal");

            migrationBuilder.RenameColumn(
                name: "Diskon",
                schema: "public",
                table: "MstObatAsuransi",
                newName: "MarkupTotal");

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonBahp",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiskonDari",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonDokter",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonJp",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonRs",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiskonSampai",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonTotal",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiskonBerlaku",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupBahp",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupDari",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupDokter",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupJp",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupLainnya",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupRs",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupSampai",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Keterangan",
                schema: "public",
                table: "MstTarifKelasAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalPemakaian",
                schema: "public",
                table: "MstTarifKelasAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonBahp",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiskonDari",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonDokter",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonJp",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonRs",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiskonSampai",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonTotal",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiskonBerlaku",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupBahp",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupDari",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupDokter",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupJp",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupLainnya",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupRs",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupSampai",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonBahp",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiskonDari",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonDokter",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonJp",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonRs",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiskonSampai",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiskonTotal",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiskonBerlaku",
                schema: "public",
                table: "MstObatAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "MstObatAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupBahp",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupDari",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupDokter",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupJp",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupLainnya",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupRs",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupSampai",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiskonBahp",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonDari",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonDokter",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonJp",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonRs",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonSampai",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonTotal",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "IsDiskonBerlaku",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupBahp",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDari",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDokter",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupJp",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupLainnya",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupRs",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupSampai",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "Keterangan",
                schema: "public",
                table: "MstTarifKelasAsuransi");

            migrationBuilder.DropColumn(
                name: "TanggalPemakaian",
                schema: "public",
                table: "MstTarifKelasAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonBahp",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonDari",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonDokter",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonJp",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonRs",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonSampai",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonTotal",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "IsDiskonBerlaku",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupBahp",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDari",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDokter",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupJp",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupLainnya",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupRs",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupSampai",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonBahp",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonDari",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonDokter",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonJp",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonRs",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonSampai",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DiskonTotal",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "IsDiskonBerlaku",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupBahp",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDari",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDokter",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupJp",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupLainnya",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupRs",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupSampai",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.RenameColumn(
                name: "MarkupTotal",
                schema: "public",
                table: "MstTindakanAsuransi",
                newName: "Diskon");

            migrationBuilder.RenameColumn(
                name: "MarkupTotal",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                newName: "Diskon");

            migrationBuilder.RenameColumn(
                name: "MarkupTotal",
                schema: "public",
                table: "MstObatAsuransi",
                newName: "Diskon");
        }
    }
}
