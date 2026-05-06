using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPathTTDHandoverPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PathTTDAdministration",
                table: "HandoverPasiens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathTTDCRO",
                table: "HandoverPasiens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathTTDPerawat",
                table: "HandoverPasiens",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathTTDAdministration",
                table: "HandoverPasiens");

            migrationBuilder.DropColumn(
                name: "PathTTDCRO",
                table: "HandoverPasiens");

            migrationBuilder.DropColumn(
                name: "PathTTDPerawat",
                table: "HandoverPasiens");
        }
    }
}
