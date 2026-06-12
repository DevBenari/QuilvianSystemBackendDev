using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomCetakFilm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CetakFilmDetail_LabPemeriksaans_PemeriksaanId",
                table: "CetakFilmDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_CetakFilmDetail_MstLab_LabId",
                table: "CetakFilmDetail");

            migrationBuilder.DropIndex(
                name: "IX_CetakFilmDetail_LabId",
                table: "CetakFilmDetail");

            migrationBuilder.DropIndex(
                name: "IX_CetakFilmDetail_PemeriksaanId",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "HargaSatuanFilm",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "HasilLab",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "HasilLabAI",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "LabId",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "NamaDokterPemeriksa",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "NamaPemeriksaan",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "NoPhoto",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "PathHasilPhoto",
                table: "CetakFilmDetail");

            migrationBuilder.DropColumn(
                name: "PemeriksaanId",
                table: "CetakFilmDetail");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierName",
                schema: "public",
                table: "Fin_PurchaseOrder",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierCode",
                schema: "public",
                table: "Fin_PurchaseOrder",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "SupplierName",
                schema: "public",
                table: "Fin_PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SupplierCode",
                schema: "public",
                table: "Fin_PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaSatuanFilm",
                table: "CetakFilmDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilLab",
                table: "CetakFilmDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilLabAI",
                table: "CetakFilmDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LabId",
                table: "CetakFilmDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaDokterPemeriksa",
                table: "CetakFilmDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPemeriksaan",
                table: "CetakFilmDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPhoto",
                table: "CetakFilmDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathHasilPhoto",
                table: "CetakFilmDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PemeriksaanId",
                table: "CetakFilmDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_LabId",
                table: "CetakFilmDetail",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_CetakFilmDetail_PemeriksaanId",
                table: "CetakFilmDetail",
                column: "PemeriksaanId");

            migrationBuilder.AddForeignKey(
                name: "FK_CetakFilmDetail_LabPemeriksaans_PemeriksaanId",
                table: "CetakFilmDetail",
                column: "PemeriksaanId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_CetakFilmDetail_MstLab_LabId",
                table: "CetakFilmDetail",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");
        }
    }
}
