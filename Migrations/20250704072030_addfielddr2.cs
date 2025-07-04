using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addfielddr2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dosis",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "DosisRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakaranDosis",
                schema: "public",
                table: "MstResepDetail",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DosisRacikan",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "TakaranDosis",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.AddColumn<string>(
                name: "Dosis",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);
        }
    }
}
