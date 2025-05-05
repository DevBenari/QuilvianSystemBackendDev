using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class useract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstObatKandungan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstObatKandungan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstObatKandungan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstObatKandungan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstObatKandungan",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstObatKandungan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstObatKandungan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstObatAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstObatAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstObatAsuransi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstObatAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstObatAsuransi",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstKandungan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstKandungan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstKandungan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstKandungan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstKandungan",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstKandungan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstKandungan",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "MstBentukObat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstBentukObat",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "MstBentukObat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstBentukObat",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstBentukObat",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "MstBentukObat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstBentukObat",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstObatKandungan");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstObatAsuransi");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstKandungan");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "MstBentukObat");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "public",
                table: "MstBentukObat");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "MstBentukObat");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "MstBentukObat");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "MstBentukObat");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "MstBentukObat");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "MstBentukObat");
        }
    }
}
