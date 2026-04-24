using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addfielddetailresep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeteranganRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakaranDosis",
                schema: "public",
                table: "MstObat",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeteranganRacikan",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "TakaranDosis",
                schema: "public",
                table: "MstObat");
        }
    }
}
