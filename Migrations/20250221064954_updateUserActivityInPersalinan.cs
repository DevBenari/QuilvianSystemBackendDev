using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class updateUserActivityInPersalinan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "dbo",
                table: "MstPersalinan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "dbo",
                table: "MstPersalinan",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "dbo",
                table: "MstPersalinan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "MstPersalinan",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "dbo",
                table: "MstPersalinan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "dbo",
                table: "MstPersalinan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "MstPersalinan",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "dbo",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "dbo",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "dbo",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "dbo",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "dbo",
                table: "MstPersalinan");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "MstPersalinan");
        }
    }
}
