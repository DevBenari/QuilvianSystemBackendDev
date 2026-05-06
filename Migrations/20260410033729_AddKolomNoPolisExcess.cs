using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomNoPolisExcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoPolis",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoPolisExcess",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoPolis",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "NoPolisExcess",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
