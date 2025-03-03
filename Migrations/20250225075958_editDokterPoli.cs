using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editDokterPoli : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JadwalPraktekId",
                table: "DokterPolis",
                newName: "UpdateBy");

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                table: "DokterPolis",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                table: "DokterPolis",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                table: "DokterPolis",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                table: "DokterPolis",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "DokterPolis",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                table: "DokterPolis",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                table: "DokterPolis");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "DokterPolis",
                newName: "JadwalPraktekId");
        }
    }
}
