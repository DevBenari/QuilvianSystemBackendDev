using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationDiTarifKelas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstTarifKelas_KelasId",
                schema: "public",
                table: "MstTarifKelas",
                column: "KelasId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTarifKelas_TindakanId",
                schema: "public",
                table: "MstTarifKelas",
                column: "TindakanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstTarifKelas_MstKelas_KelasId",
                schema: "public",
                table: "MstTarifKelas",
                column: "KelasId",
                principalSchema: "public",
                principalTable: "MstKelas",
                principalColumn: "KelasId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstTarifKelas_MstTindakan_TindakanId",
                schema: "public",
                table: "MstTarifKelas",
                column: "TindakanId",
                principalSchema: "public",
                principalTable: "MstTindakan",
                principalColumn: "TindakanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstTarifKelas_MstKelas_KelasId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropForeignKey(
                name: "FK_MstTarifKelas_MstTindakan_TindakanId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropIndex(
                name: "IX_MstTarifKelas_KelasId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropIndex(
                name: "IX_MstTarifKelas_TindakanId",
                schema: "public",
                table: "MstTarifKelas");
        }
    }
}
