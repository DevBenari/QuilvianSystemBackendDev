using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomKamarAsuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "KamarAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupBahp",
                schema: "public",
                table: "KamarAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupDari",
                schema: "public",
                table: "KamarAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupJp",
                schema: "public",
                table: "KamarAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupLainnya",
                schema: "public",
                table: "KamarAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupRs",
                schema: "public",
                table: "KamarAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarkupSampai",
                schema: "public",
                table: "KamarAsuransi",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarkupTotal",
                schema: "public",
                table: "KamarAsuransi",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMarkupBerlaku",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupBahp",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupDari",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupJp",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupLainnya",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupRs",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupSampai",
                schema: "public",
                table: "KamarAsuransi");

            migrationBuilder.DropColumn(
                name: "MarkupTotal",
                schema: "public",
                table: "KamarAsuransi");
        }
    }
}
