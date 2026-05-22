using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newTambahannes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                schema: "public",
                table: "FIN_ARHeader",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                schema: "public",
                table: "FIN_ARDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCanceled",
                schema: "public",
                table: "FIN_ARHeader");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                schema: "public",
                table: "FIN_ARDetail");
        }
    }
}
