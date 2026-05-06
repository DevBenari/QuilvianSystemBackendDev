using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ICollectionResepRacikan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RacikanDetail_ObatId",
                schema: "public",
                table: "RacikanDetail",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_RacikanDetail_RacikanId",
                schema: "public",
                table: "RacikanDetail",
                column: "RacikanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResepDetail_ObatId",
                schema: "public",
                table: "MstResepDetail",
                column: "ObatId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResepDetail_RacikanId",
                schema: "public",
                table: "MstResepDetail",
                column: "RacikanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstResepDetail_ResepId",
                schema: "public",
                table: "MstResepDetail",
                column: "ResepId");

            migrationBuilder.CreateIndex(
                name: "IX_MstRacikan_BentukRacikanId",
                schema: "public",
                table: "MstRacikan",
                column: "BentukRacikanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstRacikan_ResepId",
                schema: "public",
                table: "MstRacikan",
                column: "ResepId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstRacikan_MstRacikanBentuk_BentukRacikanId",
                schema: "public",
                table: "MstRacikan",
                column: "BentukRacikanId",
                principalSchema: "public",
                principalTable: "MstRacikanBentuk",
                principalColumn: "BentukRacikanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstRacikan_MstResep_ResepId",
                schema: "public",
                table: "MstRacikan",
                column: "ResepId",
                principalSchema: "public",
                principalTable: "MstResep",
                principalColumn: "ResepId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstResepDetail_MstObat_ObatId",
                schema: "public",
                table: "MstResepDetail",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstResepDetail_MstRacikan_RacikanId",
                schema: "public",
                table: "MstResepDetail",
                column: "RacikanId",
                principalSchema: "public",
                principalTable: "MstRacikan",
                principalColumn: "RacikanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstResepDetail_MstResep_ResepId",
                schema: "public",
                table: "MstResepDetail",
                column: "ResepId",
                principalSchema: "public",
                principalTable: "MstResep",
                principalColumn: "ResepId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RacikanDetail_MstObat_ObatId",
                schema: "public",
                table: "RacikanDetail",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RacikanDetail_MstRacikan_RacikanId",
                schema: "public",
                table: "RacikanDetail",
                column: "RacikanId",
                principalSchema: "public",
                principalTable: "MstRacikan",
                principalColumn: "RacikanId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstRacikan_MstRacikanBentuk_BentukRacikanId",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstRacikan_MstResep_ResepId",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResepDetail_MstObat_ObatId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResepDetail_MstRacikan_RacikanId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MstResepDetail_MstResep_ResepId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_RacikanDetail_MstObat_ObatId",
                schema: "public",
                table: "RacikanDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_RacikanDetail_MstRacikan_RacikanId",
                schema: "public",
                table: "RacikanDetail");

            migrationBuilder.DropIndex(
                name: "IX_RacikanDetail_ObatId",
                schema: "public",
                table: "RacikanDetail");

            migrationBuilder.DropIndex(
                name: "IX_RacikanDetail_RacikanId",
                schema: "public",
                table: "RacikanDetail");

            migrationBuilder.DropIndex(
                name: "IX_MstResepDetail_ObatId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropIndex(
                name: "IX_MstResepDetail_RacikanId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropIndex(
                name: "IX_MstResepDetail_ResepId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropIndex(
                name: "IX_MstRacikan_BentukRacikanId",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.DropIndex(
                name: "IX_MstRacikan_ResepId",
                schema: "public",
                table: "MstRacikan");
        }
    }
}
