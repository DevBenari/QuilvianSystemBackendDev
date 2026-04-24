using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ubaheditdtlresep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DosisRacikan",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "KeteranganRacikan",
                schema: "public",
                table: "MstResepDetail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DosisRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeteranganRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);
        }
    }
}
