using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamBaruDiKasirdanDetailnya : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusPembayaran",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "AngsuranKe",
                schema: "public",
                table: "MainKasirDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceBilling",
                schema: "public",
                table: "MainKasirDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KunjunganId",
                schema: "public",
                table: "MainKasirDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PasienId",
                schema: "public",
                table: "MainKasirDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SisaPembayaran",
                schema: "public",
                table: "MainKasirDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPembayaran",
                schema: "public",
                table: "MainKasirDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                schema: "public",
                table: "MainKasir",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JumlahAngsuran",
                schema: "public",
                table: "MainKasir",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoKwitansi",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PasienId",
                schema: "public",
                table: "MainKasir",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathUserVerified",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPembayaran",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TTDUserVerfiedId",
                schema: "public",
                table: "MainKasir",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AngsuranKe",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "InvoiceBilling",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "KunjunganId",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "PasienId",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "SisaPembayaran",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "TotalPembayaran",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "JumlahAngsuran",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "NoKwitansi",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "PasienId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "PathUserVerified",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "StatusPembayaran",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "TTDUserVerfiedId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.AddColumn<bool>(
                name: "StatusPembayaran",
                schema: "public",
                table: "MainKasirDetail",
                type: "boolean",
                nullable: true);
        }
    }
}
