using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddIcolectionObatdanLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MainKasirKasirId",
                schema: "public",
                table: "ObatReturn",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObatRuteDetails_RuteObatId",
                table: "ObatRuteDetails",
                column: "RuteObatId");

            migrationBuilder.CreateIndex(
                name: "IX_ObatReturnDetail_ObatId",
                schema: "public",
                table: "ObatReturnDetail",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_ObatReturnDetail_ObatReturnId",
                schema: "public",
                table: "ObatReturnDetail",
                column: "ObatReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ObatReturn_MainKasirKasirId",
                schema: "public",
                table: "ObatReturn",
                column: "MainKasirKasirId");

            migrationBuilder.CreateIndex(
                name: "IX_LogRacikPenerimaans_KunjunganId",
                table: "LogRacikPenerimaans",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LogRacikPenerimaans_ResepId",
                table: "LogRacikPenerimaans",
                column: "ResepId");

            migrationBuilder.CreateIndex(
                name: "IX_LogRacikPenerimaans_UserActiveFarmasiId",
                table: "LogRacikPenerimaans",
                column: "UserActiveFarmasiId");

            migrationBuilder.CreateIndex(
                name: "IX_LogRacikPenerimaans_UserActivePerawatId",
                table: "LogRacikPenerimaans",
                column: "UserActivePerawatId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogRacikPenerimaans_MstKunjungan_KunjunganId",
                table: "LogRacikPenerimaans",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogRacikPenerimaans_MstResep_ResepId",
                table: "LogRacikPenerimaans",
                column: "ResepId",
                principalSchema: "public",
                principalTable: "MstResep",
                principalColumn: "ResepId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogRacikPenerimaans_MstUserActive_UserActiveFarmasiId",
                table: "LogRacikPenerimaans",
                column: "UserActiveFarmasiId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogRacikPenerimaans_MstUserActive_UserActivePerawatId",
                table: "LogRacikPenerimaans",
                column: "UserActivePerawatId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ObatReturn_MainKasir_MainKasirKasirId",
                schema: "public",
                table: "ObatReturn",
                column: "MainKasirKasirId",
                principalSchema: "public",
                principalTable: "MainKasir",
                principalColumn: "KasirId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObatReturnDetail_MstObat_ObatId",
                schema: "public",
                table: "ObatReturnDetail",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ObatReturnDetail_ObatReturn_ObatReturnId",
                schema: "public",
                table: "ObatReturnDetail",
                column: "ObatReturnId",
                principalSchema: "public",
                principalTable: "ObatReturn",
                principalColumn: "ObatReturnId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObatRuteDetails_MstObatRute_RuteObatId",
                table: "ObatRuteDetails",
                column: "RuteObatId",
                principalSchema: "public",
                principalTable: "MstObatRute",
                principalColumn: "RuteObatId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogRacikPenerimaans_MstKunjungan_KunjunganId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropForeignKey(
                name: "FK_LogRacikPenerimaans_MstResep_ResepId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropForeignKey(
                name: "FK_LogRacikPenerimaans_MstUserActive_UserActiveFarmasiId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropForeignKey(
                name: "FK_LogRacikPenerimaans_MstUserActive_UserActivePerawatId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropForeignKey(
                name: "FK_ObatReturn_MainKasir_MainKasirKasirId",
                schema: "public",
                table: "ObatReturn");

            migrationBuilder.DropForeignKey(
                name: "FK_ObatReturnDetail_MstObat_ObatId",
                schema: "public",
                table: "ObatReturnDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ObatReturnDetail_ObatReturn_ObatReturnId",
                schema: "public",
                table: "ObatReturnDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ObatRuteDetails_MstObatRute_RuteObatId",
                table: "ObatRuteDetails");

            migrationBuilder.DropIndex(
                name: "IX_ObatRuteDetails_RuteObatId",
                table: "ObatRuteDetails");

            migrationBuilder.DropIndex(
                name: "IX_ObatReturnDetail_ObatId",
                schema: "public",
                table: "ObatReturnDetail");

            migrationBuilder.DropIndex(
                name: "IX_ObatReturnDetail_ObatReturnId",
                schema: "public",
                table: "ObatReturnDetail");

            migrationBuilder.DropIndex(
                name: "IX_ObatReturn_MainKasirKasirId",
                schema: "public",
                table: "ObatReturn");

            migrationBuilder.DropIndex(
                name: "IX_LogRacikPenerimaans_KunjunganId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropIndex(
                name: "IX_LogRacikPenerimaans_ResepId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropIndex(
                name: "IX_LogRacikPenerimaans_UserActiveFarmasiId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropIndex(
                name: "IX_LogRacikPenerimaans_UserActivePerawatId",
                table: "LogRacikPenerimaans");

            migrationBuilder.DropColumn(
                name: "MainKasirKasirId",
                schema: "public",
                table: "ObatReturn");
        }
    }
}
