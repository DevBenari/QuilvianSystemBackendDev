using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class HapusTglMasukDiKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TglMasuk",
                schema: "public",
                table: "MstKunjungan");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TglMasuk",
                schema: "public",
                table: "MstKunjungan",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
