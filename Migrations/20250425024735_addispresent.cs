using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addispresent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPresent",
                schema: "public",
                table: "MstKunjungan",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPresent",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
