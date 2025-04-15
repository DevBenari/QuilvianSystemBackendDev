using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editkunjungan2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Antrian",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                schema: "public",
                table: "MstKunjungan",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinished",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.AddColumn<string>(
                name: "Antrian",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
