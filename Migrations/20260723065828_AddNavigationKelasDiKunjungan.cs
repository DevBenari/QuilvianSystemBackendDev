using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationKelasDiKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstKunjungan_KelasId",
                schema: "public",
                table: "MstKunjungan",
                column: "KelasId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstKelas_KelasId",
                schema: "public",
                table: "MstKunjungan",
                column: "KelasId",
                principalSchema: "public",
                principalTable: "MstKelas",
                principalColumn: "KelasId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstKelas_KelasId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropIndex(
                name: "IX_MstKunjungan_KelasId",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
