using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class NavigasiKelassdiKamar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Kamars_KelasId",
                table: "Kamars",
                column: "KelasId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kamars_MstKelas_KelasId",
                table: "Kamars",
                column: "KelasId",
                principalSchema: "public",
                principalTable: "MstKelas",
                principalColumn: "KelasId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kamars_MstKelas_KelasId",
                table: "Kamars");

            migrationBuilder.DropIndex(
                name: "IX_Kamars_KelasId",
                table: "Kamars");
        }
    }
}
