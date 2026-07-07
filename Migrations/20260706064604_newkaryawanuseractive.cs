using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newkaryawanuseractive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgamaId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlamatDomisili",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKaryawanMedis",
                schema: "public",
                table: "MstUserActive",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KecId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KewarganegaraanId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KotaId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendidikanTerakhirId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinsiId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusKewarganegaraan",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPerkawinan",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgamaId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "AlamatDomisili",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "IsKaryawanMedis",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KecId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KewarganegaraanId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KotaId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "PendidikanTerakhirId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "ProvinsiId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "StatusKewarganegaraan",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "StatusPerkawinan",
                schema: "public",
                table: "MstUserActive");

        }
    }
}
