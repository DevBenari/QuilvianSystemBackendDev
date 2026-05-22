using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class RevisiTableKunjunganLayanan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjunganLayanan_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjunganLayanan_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan");

            migrationBuilder.DropColumn(
                name: "RanapId",
                schema: "public",
                table: "MstKunjunganLayanan");

            migrationBuilder.AlterColumn<Guid>(
                name: "KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjunganLayanan_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "InstalasiUnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjunganLayanan_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjunganLayanan_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjunganLayanan_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan");

            migrationBuilder.AlterColumn<Guid>(
                name: "KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                schema: "public",
                table: "MstKunjunganLayanan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjunganLayanan_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "InstalasiUnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjunganLayanan_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
