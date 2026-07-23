using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationTindakanKunjunganPart2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TindakanKunjungans_KunjunganId",
                table: "TindakanKunjungans",
                column: "KunjunganId");

            migrationBuilder.AddForeignKey(
                name: "FK_TindakanKunjungans_MstKunjungan_KunjunganId",
                table: "TindakanKunjungans",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TindakanKunjungans_MstKunjungan_KunjunganId",
                table: "TindakanKunjungans");

            migrationBuilder.DropIndex(
                name: "IX_TindakanKunjungans_KunjunganId",
                table: "TindakanKunjungans");
        }
    }
}
