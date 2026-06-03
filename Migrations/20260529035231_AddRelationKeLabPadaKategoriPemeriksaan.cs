using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddRelationKeLabPadaKategoriPemeriksaan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LabKategoriPemeriksaans_LabId",
                table: "LabKategoriPemeriksaans",
                column: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabKategoriPemeriksaans_MstLab_LabId",
                table: "LabKategoriPemeriksaans",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabKategoriPemeriksaans_MstLab_LabId",
                table: "LabKategoriPemeriksaans");

            migrationBuilder.DropIndex(
                name: "IX_LabKategoriPemeriksaans_LabId",
                table: "LabKategoriPemeriksaans");
        }
    }
}
