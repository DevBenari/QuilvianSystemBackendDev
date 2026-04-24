using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomVitalSignPdfPasienBaru : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IramaNapas",
                table: "PengkajianPernapasans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdaRetraksiDada",
                table: "PengkajianPernapasans",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdaSesakNapas",
                table: "PengkajianPernapasans",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGerakanDadaSimetris",
                table: "PengkajianPernapasans",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsJalanNapasPaten",
                table: "PengkajianPernapasans",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPolaNapasTeratur",
                table: "PengkajianPernapasans",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Obstruksi",
                table: "PengkajianPernapasans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuaraNapas",
                table: "PengkajianPernapasans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinggalBersama",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNadiTeraba",
                schema: "public",
                table: "MstVitalSign",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kesadaran",
                schema: "public",
                table: "MstVitalSign",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFarmakologi",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMenggunakanPenopang",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPengobatanSaatIni",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTubuhTidakSeimbang",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeluhanTambahan",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KategoriTindakan",
                table: "IGDTindakanDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TTDPath",
                table: "IGDTindakanDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaktuTindakan",
                table: "IGDTindakanDetails",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IramaNapas",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "IsAdaRetraksiDada",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "IsAdaSesakNapas",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "IsGerakanDadaSimetris",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "IsJalanNapasPaten",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "IsPolaNapasTeratur",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "Obstruksi",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "SuaraNapas",
                table: "PengkajianPernapasans");

            migrationBuilder.DropColumn(
                name: "TinggalBersama",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "IsNadiTeraba",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "Kesadaran",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "IsFarmakologi",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsMenggunakanPenopang",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsPengobatanSaatIni",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsTubuhTidakSeimbang",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "KeluhanTambahan",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "KategoriTindakan",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "TTDPath",
                table: "IGDTindakanDetails");

            migrationBuilder.DropColumn(
                name: "WaktuTindakan",
                table: "IGDTindakanDetails");
        }
    }
}
