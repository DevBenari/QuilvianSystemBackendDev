using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiTableMasterBakteriAntiBiotik : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamaSubBakteri",
                table: "MstSubBakteris",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaBakteri",
                table: "MstBakteris",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaAntibiotik",
                table: "MstAntibiotiks",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaSubBakteri",
                table: "MstSubBakteris");

            migrationBuilder.DropColumn(
                name: "NamaBakteri",
                table: "MstBakteris");

            migrationBuilder.DropColumn(
                name: "NamaAntibiotik",
                table: "MstAntibiotiks");
        }
    }
}
