using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class updateobat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaraKerja",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dosis",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Farmakologi",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Indikasi",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InteraksiObat",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kontraindikasi",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Maximal",
                schema: "public",
                table: "MstObat",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Minimal",
                schema: "public",
                table: "MstObat",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Peringatan",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaraKerja",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Dosis",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Farmakologi",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Indikasi",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "InteraksiObat",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Kontraindikasi",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Maximal",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Minimal",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Peringatan",
                schema: "public",
                table: "MstObat");
        }
    }
}
