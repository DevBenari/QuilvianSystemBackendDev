using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addkolomracikan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Qty",
                schema: "public",
                table: "RacikanDetail",
                newName: "QtyUsed");

            migrationBuilder.AddColumn<int>(
                name: "QtyRacikan",
                schema: "public",
                table: "MstRacikan",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QtyRacikan",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.RenameColumn(
                name: "QtyUsed",
                schema: "public",
                table: "RacikanDetail",
                newName: "Qty");
        }
    }
}
