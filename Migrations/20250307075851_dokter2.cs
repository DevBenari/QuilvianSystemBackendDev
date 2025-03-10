using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class dokter2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "JudulFileFoto",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "FotoDokter",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.RenameColumn(
                name: "JudulFileFoto",
                schema: "public",
                table: "MstDokter",
                newName: "FotoName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FotoName",
                schema: "public",
                table: "MstDokter",
                newName: "JudulFileFoto");


            migrationBuilder.AddColumn<string>(
                name: "JudulFileFoto",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoDokter",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageBytes",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);
        }
    }
}
