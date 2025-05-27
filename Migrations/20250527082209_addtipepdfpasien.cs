using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtipepdfpasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipePendaftaran",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstResepTemplate_ObatId",
                schema: "public",
                table: "MstResepTemplate",
                column: "ObatId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstResepTemplate_MstObat_ObatId",
                schema: "public",
                table: "MstResepTemplate",
                column: "ObatId",
                principalSchema: "public",
                principalTable: "MstObat",
                principalColumn: "ObatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstResepTemplate_MstObat_ObatId",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstResepTemplate_ObatId",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "TipePendaftaran",
                schema: "public",
                table: "PdfPasienBaru");
        }
    }
}
