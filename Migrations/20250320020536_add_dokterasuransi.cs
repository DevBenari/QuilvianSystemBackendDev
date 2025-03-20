using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class add_dokterasuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterAsuransi_MstDokter_DokterId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstDokterAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstDokterAsuransi_DokterId",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropColumn(
                name: "KodeDokterAsuransi",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.DropColumn(
                name: "NamaAsuransi",
                schema: "public",
                table: "MstDokterAsuransi");

            migrationBuilder.AlterColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "KodeDokterAsuransi",
                schema: "public",
                table: "MstDokterAsuransi",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaAsuransi",
                schema: "public",
                table: "MstDokterAsuransi",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterAsuransi_DokterId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterAsuransi_MstDokter_DokterId",
                schema: "public",
                table: "MstDokterAsuransi",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
