using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomUserActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GolonganDarahId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaBank",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPolisAsuransi",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorRekening",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GolonganDarahId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NamaBank",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NoPolisAsuransi",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "NomorRekening",
                schema: "public",
                table: "MstUserActive");
        }
    }
}
