using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomStatusDiIGDTriageNosokomial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Nosokomials",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "IGDTriages",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "IGDPasienDetails",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Nosokomials");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IGDTriages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IGDPasienDetails");
        }
    }
}
