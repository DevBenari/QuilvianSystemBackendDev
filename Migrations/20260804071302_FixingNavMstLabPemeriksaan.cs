using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class FixingNavMstLabPemeriksaan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstPemeriksaanAsuransi_LabPemeriksaans_LabPemeriksaanPemeri~",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstPemeriksaanAsuransi_LabPemeriksaanPemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropColumn(
                name: "LabPemeriksaanPemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.CreateIndex(
                name: "IX_MstPemeriksaanAsuransi_PemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                column: "PemeriksaanLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstPemeriksaanAsuransi_LabPemeriksaans_PemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                column: "PemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstPemeriksaanAsuransi_LabPemeriksaans_PemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstPemeriksaanAsuransi_PemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.AddColumn<Guid>(
                name: "LabPemeriksaanPemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPemeriksaanAsuransi_LabPemeriksaanPemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                column: "LabPemeriksaanPemeriksaanLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstPemeriksaanAsuransi_LabPemeriksaans_LabPemeriksaanPemeri~",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                column: "LabPemeriksaanPemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId");
        }
    }
}
