using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomKajianPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IndikasiTindakLanjut",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KamarId",
                schema: "public",
                table: "KajianPasien",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaTempat",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PenyampaianEdukasi",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglTindakLanjut",
                schema: "public",
                table: "KajianPasien",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndikasiTindakLanjut",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KamarId",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "NamaTempat",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "PenyampaianEdukasi",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "TglTindakLanjut",
                schema: "public",
                table: "KajianPasien");
        }
    }
}
