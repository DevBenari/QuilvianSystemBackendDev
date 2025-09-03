using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamResepTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Deskripsi",
                schema: "public",
                table: "MstResepTemplate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosa",
                schema: "public",
                table: "MstResepTemplate",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deskripsi",
                schema: "public",
                table: "MstResepTemplate");

            migrationBuilder.DropColumn(
                name: "Diagnosa",
                schema: "public",
                table: "MstResepTemplate");
        }
    }
}
