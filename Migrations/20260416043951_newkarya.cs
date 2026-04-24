using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newkarya : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alamat",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankId",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartementId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoName",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JabatanId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodeKaryawan",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoHandphone",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoIdentitas",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoKaryawan",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoRekening",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalAkhirKerja",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalAwalKerja",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalKontrak",
                schema: "public",
                table: "MstUserActive",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alamat",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "BankId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "DepartementId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "FotoName",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "JabatanId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "KodeKaryawan",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NoHandphone",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NoIdentitas",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NoKaryawan",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NoRekening",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TanggalAkhirKerja",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TanggalAwalKerja",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TanggalKontrak",
                schema: "public",
                table: "MstUserActive");
        }
    }
}
