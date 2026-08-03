using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationDiMstAsuransiPemeriksaanLab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LabPemeriksaanPemeriksaanLabId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPemeriksaanAsuransi_AsuransiId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                column: "AsuransiId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MstPemeriksaanAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstPemeriksaanAsuransi",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstPemeriksaanAsuransi_LabPemeriksaans_LabPemeriksaanPemeri~",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropForeignKey(
                name: "FK_MstPemeriksaanAsuransi_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstPemeriksaanAsuransi");

            migrationBuilder.DropIndex(
                name: "IX_MstPemeriksaanAsuransi_AsuransiId",
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
        }
    }
}
