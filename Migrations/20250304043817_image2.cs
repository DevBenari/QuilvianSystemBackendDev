using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class image2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                  name: "ImageBytes",
                  table: "PdfPasienBaru");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageBytes",
                schema: "public",
                table: "PdfPasienBaru",
                type: "BYTEA",
                nullable: true);
        }
    }
}
