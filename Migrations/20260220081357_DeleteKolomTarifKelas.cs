using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class DeleteKolomTarifKelas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DokterId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "PemeriksaanLabId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "PeralatanId",
                schema: "public",
                table: "MstTarifKelas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstTarifKelas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PemeriksaanLabId",
                schema: "public",
                table: "MstTarifKelas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PeralatanId",
                schema: "public",
                table: "MstTarifKelas",
                type: "uuid",
                nullable: true);
        }
    }
}
