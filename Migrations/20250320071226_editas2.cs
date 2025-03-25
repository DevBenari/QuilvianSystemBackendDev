using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editas2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstAsuransi");
        }
    }
}
