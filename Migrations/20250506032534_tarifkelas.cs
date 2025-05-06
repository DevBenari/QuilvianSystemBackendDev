using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class tarifkelas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaKelas",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "NamaPoliklinik",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "PoliklinikId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.RenameColumn(
                name: "TindakanPoliId",
                schema: "public",
                table: "MstTarifKelas",
                newName: "TindakanId");

            migrationBuilder.RenameColumn(
                name: "TarifTindakanId",
                schema: "public",
                table: "MstTarifKelas",
                newName: "TarifKelasId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TindakanId",
                schema: "public",
                table: "MstTarifKelas",
                newName: "TindakanPoliId");

            migrationBuilder.RenameColumn(
                name: "TarifKelasId",
                schema: "public",
                table: "MstTarifKelas",
                newName: "TarifTindakanId");

            migrationBuilder.AddColumn<string>(
                name: "NamaKelas",
                schema: "public",
                table: "MstTarifKelas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPoliklinik",
                schema: "public",
                table: "MstTarifKelas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PoliklinikId",
                schema: "public",
                table: "MstTarifKelas",
                type: "uuid",
                nullable: true);
        }
    }
}
