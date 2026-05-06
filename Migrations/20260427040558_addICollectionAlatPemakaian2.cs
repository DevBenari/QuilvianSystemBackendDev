using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addICollectionAlatPemakaian2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaians_KunjunganId",
                table: "AlatPemakaians",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaians_PasienId",
                table: "AlatPemakaians",
                column: "PasienId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaians_MstKunjungan_KunjunganId",
                table: "AlatPemakaians",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaians_PdfPasienBaru_PasienId",
                table: "AlatPemakaians",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaians_MstKunjungan_KunjunganId",
                table: "AlatPemakaians");

            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaians_PdfPasienBaru_PasienId",
                table: "AlatPemakaians");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaians_KunjunganId",
                table: "AlatPemakaians");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaians_PasienId",
                table: "AlatPemakaians");
        }
    }
}
