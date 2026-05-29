using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddRelationKeKategoriPemeriksaan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LabPemeriksaans_KategoriPemeriksaanId",
                table: "LabPemeriksaans",
                column: "KategoriPemeriksaanId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabPemeriksaans_LabKategoriPemeriksaans_KategoriPemeriksaan~",
                table: "LabPemeriksaans",
                column: "KategoriPemeriksaanId",
                principalTable: "LabKategoriPemeriksaans",
                principalColumn: "KategoriPemeriksaanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabPemeriksaans_LabKategoriPemeriksaans_KategoriPemeriksaan~",
                table: "LabPemeriksaans");

            migrationBuilder.DropIndex(
                name: "IX_LabPemeriksaans_KategoriPemeriksaanId",
                table: "LabPemeriksaans");
        }
    }
}
