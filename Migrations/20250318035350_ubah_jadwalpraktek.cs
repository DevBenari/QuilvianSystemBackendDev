using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ubah_jadwalpraktek : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstJadwalPraktek_DokterPolis_DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropIndex(
                name: "IX_MstJadwalPraktek_DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "DokterId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "PoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.AlterColumn<Guid>(
                name: "DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId",
                principalSchema: "public",
                principalTable: "MstDokterSubPoli",
                principalColumn: "DokterSubPoliId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.AlterColumn<Guid>(
                name: "DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterPoliId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstJadwalPraktek_DokterPolis_DokterPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterPoliId",
                principalTable: "DokterPolis",
                principalColumn: "DokterPoliId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId",
                principalSchema: "public",
                principalTable: "MstDokterSubPoli",
                principalColumn: "DokterSubPoliId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
