using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddIcolectionObatdanSatuanBentuk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstObat_BentukObatId",
                schema: "public",
                table: "MstObat",
                column: "BentukObatId");

            migrationBuilder.CreateIndex(
                name: "IX_MstObat_SatuanId",
                schema: "public",
                table: "MstObat",
                column: "SatuanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstObat_MstBentukObat_BentukObatId",
                schema: "public",
                table: "MstObat",
                column: "BentukObatId",
                principalSchema: "public",
                principalTable: "MstBentukObat",
                principalColumn: "BentukSatuanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstObat_MstSatuan_SatuanId",
                schema: "public",
                table: "MstObat",
                column: "SatuanId",
                principalSchema: "public",
                principalTable: "MstSatuan",
                principalColumn: "SatuanId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstObat_MstBentukObat_BentukObatId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropForeignKey(
                name: "FK_MstObat_MstSatuan_SatuanId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropIndex(
                name: "IX_MstObat_BentukObatId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropIndex(
                name: "IX_MstObat_SatuanId",
                schema: "public",
                table: "MstObat");
        }
    }
}
