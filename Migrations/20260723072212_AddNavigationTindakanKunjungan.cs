using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationTindakanKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TindakanKunjungans_KelasId",
                table: "TindakanKunjungans",
                column: "KelasId");

            migrationBuilder.CreateIndex(
                name: "IX_TindakanKunjungans_TindakanId",
                table: "TindakanKunjungans",
                column: "TindakanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TindakanKunjungans_MstKelas_KelasId",
                table: "TindakanKunjungans",
                column: "KelasId",
                principalSchema: "public",
                principalTable: "MstKelas",
                principalColumn: "KelasId");

            migrationBuilder.AddForeignKey(
                name: "FK_TindakanKunjungans_MstTindakan_TindakanId",
                table: "TindakanKunjungans",
                column: "TindakanId",
                principalSchema: "public",
                principalTable: "MstTindakan",
                principalColumn: "TindakanId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TindakanKunjungans_MstKelas_KelasId",
                table: "TindakanKunjungans");

            migrationBuilder.DropForeignKey(
                name: "FK_TindakanKunjungans_MstTindakan_TindakanId",
                table: "TindakanKunjungans");

            migrationBuilder.DropIndex(
                name: "IX_TindakanKunjungans_KelasId",
                table: "TindakanKunjungans");

            migrationBuilder.DropIndex(
                name: "IX_TindakanKunjungans_TindakanId",
                table: "TindakanKunjungans");
        }
    }
}
