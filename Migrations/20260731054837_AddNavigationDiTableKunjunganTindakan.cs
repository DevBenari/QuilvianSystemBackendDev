using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationDiTableKunjunganTindakan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TindakanKunjungans_DokterPemeriksaId",
                table: "TindakanKunjungans",
                column: "DokterPemeriksaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TindakanKunjungans_MstDokter_DokterPemeriksaId",
                table: "TindakanKunjungans",
                column: "DokterPemeriksaId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TindakanKunjungans_MstDokter_DokterPemeriksaId",
                table: "TindakanKunjungans");

            migrationBuilder.DropIndex(
                name: "IX_TindakanKunjungans_DokterPemeriksaId",
                table: "TindakanKunjungans");
        }
    }
}
