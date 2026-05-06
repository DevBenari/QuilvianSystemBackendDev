using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomBookRuangBedah : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusOperasi",
                table: "RuangBedahBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaraMasukRS",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KondisiKeluar",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglMasuk",
                schema: "public",
                table: "MstKunjungan",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusOperasi",
                table: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "CaraMasukRS",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "KondisiKeluar",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "TglMasuk",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
