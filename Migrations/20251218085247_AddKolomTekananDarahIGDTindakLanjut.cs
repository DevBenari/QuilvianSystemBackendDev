using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTekananDarahIGDTindakLanjut : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TD",
                table: "IGDTindakLanjuts");

            migrationBuilder.AddColumn<decimal>(
                name: "TekananDarahDiastolic",
                table: "IGDTindakLanjuts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TekananDarahSystolic",
                table: "IGDTindakLanjuts",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TekananDarahDiastolic",
                table: "IGDTindakLanjuts");

            migrationBuilder.DropColumn(
                name: "TekananDarahSystolic",
                table: "IGDTindakLanjuts");

            migrationBuilder.AddColumn<string>(
                name: "TD",
                table: "IGDTindakLanjuts",
                type: "text",
                nullable: true);
        }
    }
}
