using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class edittableuseractive2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomorTelepon",
                schema: "public",
                table: "MstUserActive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomorTelepon",
                schema: "public",
                table: "MstUserActive",
                type: "text",
                nullable: true);
        }
    }
}
