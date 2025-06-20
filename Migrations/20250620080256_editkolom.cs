using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editkolom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InteraturObat",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "InteraturObat",
                schema: "public",
                table: "MstResep",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                schema: "public",
                table: "MstResep",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InteraturObat",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                schema: "public",
                table: "MstResep");

            migrationBuilder.AddColumn<string>(
                name: "InteraturObat",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);
        }
    }
}
