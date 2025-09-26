using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiPraOperasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PenandaanOperasiBag1",
                table: "PraOperasis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PenandaanOperasiBag2",
                table: "PraOperasis",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenandaanOperasiBag1",
                table: "PraOperasis");

            migrationBuilder.DropColumn(
                name: "PenandaanOperasiBag2",
                table: "PraOperasis");
        }
    }
}
