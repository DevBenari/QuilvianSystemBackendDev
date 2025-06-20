using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hpsklmnamametode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaMetode",
                schema: "public",
                table: "MainKasir");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamaMetode",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);
        }
    }
}
