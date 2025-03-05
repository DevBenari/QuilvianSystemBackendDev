using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class changeKewarganegaraan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KewarganegaraanId",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.AddColumn<string>(
                name: "Kewarganegaraan",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kewarganegaraan",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.AddColumn<Guid>(
                name: "KewarganegaraanId",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
