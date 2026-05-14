using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationUntukGudang2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStockGudangUnit",
                table: "GudangUnits");

            migrationBuilder.DropColumn(
                name: "MinStockGudangUnit",
                table: "GudangUnits");

            migrationBuilder.DropColumn(
                name: "StockGudangUnit",
                table: "GudangUnits");

            migrationBuilder.DropColumn(
                name: "StockPenyanggaGudangUnit",
                table: "GudangUnits");

            migrationBuilder.RenameColumn(
                name: "ObatId",
                table: "GudangUnits",
                newName: "InstalasiUnitId");

            migrationBuilder.AddColumn<string>(
                name: "KodeGudangUnit",
                table: "GudangUnits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaGudangUnit",
                table: "GudangUnits",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GudangUnits_InstalasiUnitId",
                table: "GudangUnits",
                column: "InstalasiUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_GudangUnits_Hrd_InstalasiUnit_InstalasiUnitId",
                table: "GudangUnits",
                column: "InstalasiUnitId",
                principalSchema: "public",
                principalTable: "Hrd_InstalasiUnit",
                principalColumn: "InstalasiUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GudangUnits_Hrd_InstalasiUnit_InstalasiUnitId",
                table: "GudangUnits");

            migrationBuilder.DropIndex(
                name: "IX_GudangUnits_InstalasiUnitId",
                table: "GudangUnits");

            migrationBuilder.DropColumn(
                name: "KodeGudangUnit",
                table: "GudangUnits");

            migrationBuilder.DropColumn(
                name: "NamaGudangUnit",
                table: "GudangUnits");

            migrationBuilder.RenameColumn(
                name: "InstalasiUnitId",
                table: "GudangUnits",
                newName: "ObatId");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxStockGudangUnit",
                table: "GudangUnits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinStockGudangUnit",
                table: "GudangUnits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockGudangUnit",
                table: "GudangUnits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockPenyanggaGudangUnit",
                table: "GudangUnits",
                type: "numeric",
                nullable: true);
        }
    }
}
