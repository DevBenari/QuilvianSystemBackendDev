using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editDokter2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FotoBase64",
                schema: "dbo",
                table: "MstDokter",
                newName: "JudulFileFoto");

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "dbo",
                table: "MstDokter",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                schema: "dbo",
                table: "MstDokter",
                type: "VARBINARY(MAX)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.RenameColumn(
                name: "JudulFileFoto",
                schema: "dbo",
                table: "MstDokter",
                newName: "FotoBase64");
        }
    }
}
