using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editdetailracikan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DetailResepId",
                schema: "public",
                table: "RacikanDetail",
                newName: "RacikanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RacikanId",
                schema: "public",
                table: "RacikanDetail",
                newName: "DetailResepId");
        }
    }
}
