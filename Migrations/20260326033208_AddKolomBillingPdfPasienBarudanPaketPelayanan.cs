using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomBillingPdfPasienBarudanPaketPelayanan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LayananId",
                schema: "public",
                table: "MstPaketLayanan");

            migrationBuilder.AddColumn<string>(
                name: "NoKaryawan",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMCU",
                schema: "public",
                table: "MstPaketLayanan",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Umur",
                schema: "public",
                table: "MstAsuransiPasien",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsUtama",
                schema: "public",
                table: "MstAsuransiPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoveragePercentage",
                schema: "public",
                table: "Billing",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LayananId",
                schema: "public",
                table: "Billing",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoKaryawan",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "IsMCU",
                schema: "public",
                table: "MstPaketLayanan");

            migrationBuilder.DropColumn(
                name: "IsUtama",
                schema: "public",
                table: "MstAsuransiPasien");

            migrationBuilder.DropColumn(
                name: "CoveragePercentage",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "LayananId",
                schema: "public",
                table: "Billing");

            migrationBuilder.AddColumn<Guid>(
                name: "LayananId",
                schema: "public",
                table: "MstPaketLayanan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Umur",
                schema: "public",
                table: "MstAsuransiPasien",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
