using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationUntukGudang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstObatUnit_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstObatUnit");

            migrationBuilder.RenameColumn(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstObatUnit",
                newName: "GudangUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_MstObatUnit_InstalasiUnitId",
                schema: "public",
                table: "MstObatUnit",
                newName: "IX_MstObatUnit_GudangUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GudangUnits_GudangId",
                table: "GudangUnits",
                column: "GudangId");

            migrationBuilder.AddForeignKey(
                name: "FK_GudangUnits_MstGudang_GudangId",
                table: "GudangUnits",
                column: "GudangId",
                principalSchema: "public",
                principalTable: "MstGudang",
                principalColumn: "GudangId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstObatUnit_GudangUnits_GudangUnitId",
                schema: "public",
                table: "MstObatUnit",
                column: "GudangUnitId",
                principalTable: "GudangUnits",
                principalColumn: "GudangUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GudangUnits_MstGudang_GudangId",
                table: "GudangUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_MstObatUnit_GudangUnits_GudangUnitId",
                schema: "public",
                table: "MstObatUnit");

            migrationBuilder.DropIndex(
                name: "IX_GudangUnits_GudangId",
                table: "GudangUnits");

            migrationBuilder.RenameColumn(
                name: "GudangUnitId",
                schema: "public",
                table: "MstObatUnit",
                newName: "InstalasiUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_MstObatUnit_GudangUnitId",
                schema: "public",
                table: "MstObatUnit",
                newName: "IX_MstObatUnit_InstalasiUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstObatUnit_Hrd_InstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstObatUnit",
                column: "InstalasiUnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId");
        }
    }
}
