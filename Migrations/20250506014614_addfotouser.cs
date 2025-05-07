using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addfotouser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoName",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoName",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                schema: "public",
                table: "MstUserActive");
        }
    }
}
