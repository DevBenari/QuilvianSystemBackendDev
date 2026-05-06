using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomKajianPasienIsAsing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAsing",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDaerah",
                schema: "public",
                table: "KajianPasien",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAsing",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "IsDaerah",
                schema: "public",
                table: "KajianPasien");
        }
    }
}
