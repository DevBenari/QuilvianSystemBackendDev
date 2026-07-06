using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationDiUserActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartementId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstUserActive_DepartemenId",
                schema: "public",
                table: "MstUserActive",
                column: "DepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_MstUserActive_InstalasiUnitId",
                schema: "public",
                table: "MstUserActive",
                column: "InstalasiUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstUserActive_JabatanId",
                schema: "public",
                table: "MstUserActive",
                column: "JabatanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstUserActive_PositionId",
                schema: "public",
                table: "MstUserActive",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstUserActive_TipeUserId",
                schema: "public",
                table: "MstUserActive",
                column: "TipeUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstUserActive_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstUserActive",
                column: "InstalasiUnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstUserActive_MstDepartement_DepartemenId",
                schema: "public",
                table: "MstUserActive",
                column: "DepartemenId",
                principalSchema: "public",
                principalTable: "MstDepartement",
                principalColumn: "DepartementId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstUserActive_MstJabatan_JabatanId",
                schema: "public",
                table: "MstUserActive",
                column: "JabatanId",
                principalSchema: "public",
                principalTable: "MstJabatan",
                principalColumn: "JabatanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstUserActive_MstPosition_PositionId",
                schema: "public",
                table: "MstUserActive",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstUserActive_MstTipeUser_TipeUserId",
                schema: "public",
                table: "MstUserActive",
                column: "TipeUserId",
                principalSchema: "public",
                principalTable: "MstTipeUser",
                principalColumn: "TipeUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstUserActive_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropForeignKey(
                name: "FK_MstUserActive_MstDepartement_DepartemenId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropForeignKey(
                name: "FK_MstUserActive_MstJabatan_JabatanId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropForeignKey(
                name: "FK_MstUserActive_MstPosition_PositionId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropForeignKey(
                name: "FK_MstUserActive_MstTipeUser_TipeUserId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropIndex(
                name: "IX_MstUserActive_DepartemenId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropIndex(
                name: "IX_MstUserActive_InstalasiUnitId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropIndex(
                name: "IX_MstUserActive_JabatanId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropIndex(
                name: "IX_MstUserActive_PositionId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropIndex(
                name: "IX_MstUserActive_TipeUserId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartementId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);
        }
    }
}
