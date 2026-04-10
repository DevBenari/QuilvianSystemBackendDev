using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddAsuransiPasienIdKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoPolis",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "NoPolisExcess",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.AddColumn<string>(
                name: "NoPolis",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPolisExcess",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);
        }
    }
}
