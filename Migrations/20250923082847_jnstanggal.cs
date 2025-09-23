using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class jnstanggal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bln",
                schema: "public",
                table: "JnsPembayaran");

            migrationBuilder.AlterColumn<string>(
                name: "TanggalMasuk",
                schema: "public",
                table: "JnsPembayaran",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "TanggalKeluar",
                schema: "public",
                table: "JnsPembayaran",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "JenisTanggal",
                schema: "public",
                table: "JnsPembayaran",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JenisTanggal",
                schema: "public",
                table: "JnsPembayaran");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalMasuk",
                schema: "public",
                table: "JnsPembayaran",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalKeluar",
                schema: "public",
                table: "JnsPembayaran",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bln",
                schema: "public",
                table: "JnsPembayaran",
                type: "text",
                nullable: true);
        }
    }
}
