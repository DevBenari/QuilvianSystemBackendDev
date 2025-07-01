using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class resepdosis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dosis",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dosis",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "JenisRacikan",
                schema: "public",
                table: "MstResepDetail");
        }
    }
}
