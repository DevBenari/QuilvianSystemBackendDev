using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class edittableresep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InteraturObat",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                schema: "public",
                table: "MstResep");

            migrationBuilder.AddColumn<bool>(
                name: "IsIteratur",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JarakPenebusan",
                schema: "public",
                table: "MstResepDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "JumlahIteratur",
                schema: "public",
                table: "MstResepDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MasaAktifIteratur",
                schema: "public",
                table: "MstResepDetail",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TglMulaiIteratur",
                schema: "public",
                table: "MstResepDetail",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIteratur",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "JarakPenebusan",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "JumlahIteratur",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "MasaAktifIteratur",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "TglMulaiIteratur",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "InteraturObat",
                schema: "public",
                table: "MstResep",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                schema: "public",
                table: "MstResep",
                type: "boolean",
                nullable: true);
        }
    }
}
