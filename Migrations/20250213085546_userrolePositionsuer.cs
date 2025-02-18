using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class userrolePositionsuer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "AspNetRolePosition",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "AspNetRoleDepartemen",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "AspNetRolePosition");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "dbo",
                table: "AspNetRoleDepartemen");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                schema: "dbo",
                table: "AspNetRoleDepartemen");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "dbo",
                table: "AspNetRoleDepartemen");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "dbo",
                table: "AspNetRoleDepartemen");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "dbo",
                table: "AspNetRoleDepartemen");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "dbo",
                table: "AspNetRoleDepartemen");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "dbo",
                table: "AspNetRoleDepartemen");
        }
    }
}
