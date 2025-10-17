using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editkolomstatusresep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusPengambilan",
                schema: "public",
                table: "MstResep");

            migrationBuilder.AddColumn<bool>(
                name: "StatusPengambilanObat",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPengambilanResep",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusPengambilanObat",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "StatusPengambilanResep",
                schema: "public",
                table: "MstResep");

            migrationBuilder.AddColumn<bool>(
                name: "StatusPengambilan",
                schema: "public",
                table: "MstResep",
                type: "boolean",
                nullable: true);
        }
    }
}
