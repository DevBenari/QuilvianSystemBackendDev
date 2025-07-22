using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addparammstobat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HargaJual",
                schema: "public",
                table: "MstObat",
                newName: "HTEPrice");

            migrationBuilder.RenameColumn(
                name: "HargaAwal",
                schema: "public",
                table: "MstObat",
                newName: "HNAPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "Cogs",
                schema: "public",
                table: "MstObat",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cogs",
                schema: "public",
                table: "MstObat");

            migrationBuilder.RenameColumn(
                name: "HTEPrice",
                schema: "public",
                table: "MstObat",
                newName: "HargaJual");

            migrationBuilder.RenameColumn(
                name: "HNAPrice",
                schema: "public",
                table: "MstObat",
                newName: "HargaAwal");
        }
    }
}
