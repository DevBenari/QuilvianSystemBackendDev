using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editas4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Createdate",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.AddColumn<string>(
                name: "TanggalRegist",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TanggalRegist",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.AddColumn<DateTime>(
                name: "Createdate",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
