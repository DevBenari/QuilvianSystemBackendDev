using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTekananDarahIGDObserv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TD",
                table: "IGDObservasiDetails",
                newName: "TekananDarahSystolic");

            migrationBuilder.AddColumn<decimal>(
                name: "TekananDarahDiastolic",
                table: "IGDObservasiDetails",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TekananDarahDiastolic",
                table: "IGDObservasiDetails");

            migrationBuilder.RenameColumn(
                name: "TekananDarahSystolic",
                table: "IGDObservasiDetails",
                newName: "TD");
        }
    }
}
