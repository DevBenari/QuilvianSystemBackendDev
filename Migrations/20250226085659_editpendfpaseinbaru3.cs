using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editpendfpaseinbaru3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FotoBase64",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "VARBINARY(MAX)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBytes",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.AlterColumn<string>(
                name: "FotoBase64",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);
        }
    }
}
