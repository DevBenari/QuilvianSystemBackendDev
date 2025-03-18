using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ubah_datetime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstAsuransi_AsuransiId",
                table: "DokterPolis");

            migrationBuilder.DropForeignKey(
                name: "FK_DokterPolis_MstPoliklinik_PoliId",
                table: "DokterPolis");

            migrationBuilder.DropForeignKey(
                name: "FK_MstCoveranAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstCoveranAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstCoveranAsuransi_AsuransiId",
                schema: "public",
                table: "MstCoveranAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_AsuransiId",
                table: "DokterPolis");

            migrationBuilder.DropIndex(
                name: "IX_DokterPolis_PoliId",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "AsuransiId",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "KodeDokterPoli",
                table: "DokterPolis");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiId",
                table: "DokterPolis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodeDokterPoli",
                table: "DokterPolis",
                type: "text",
                nullable: true);


            migrationBuilder.AddColumn<string>(
                name: "NamaPoliKlinik",
                table: "DokterPolis",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCoveranAsuransi_AsuransiId",
                schema: "public",
                table: "MstCoveranAsuransi",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_AsuransiId",
                table: "DokterPolis",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_PoliId",
                table: "DokterPolis",
                column: "PoliId");

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPolis_MstAsuransi_AsuransiId",
                table: "DokterPolis",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");


            migrationBuilder.AddForeignKey(
                name: "FK_MstCoveranAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstCoveranAsuransi",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");
        }
    }
}
