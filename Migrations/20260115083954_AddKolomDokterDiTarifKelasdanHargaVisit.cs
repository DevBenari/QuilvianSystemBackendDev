using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDokterDiTarifKelasdanHargaVisit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstTarifKelas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaVisit",
                schema: "public",
                table: "MstDokter",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DokterId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "HargaVisit",
                schema: "public",
                table: "MstDokter");
        }
    }
}
