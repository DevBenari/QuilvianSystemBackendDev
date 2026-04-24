using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class req1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LokasiPenempatan",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.AddColumn<Guid>(
                name: "PendidikanId",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinsiId",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "status",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendidikanId",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "ProvinsiId",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.AddColumn<string>(
                name: "LokasiPenempatan",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "text",
                nullable: true);
        }
    }
}
