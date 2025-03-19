using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ubah_jadwalpraktek_timespan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KodeJadwalPraktek",
                schema: "public",
                table: "MstJadwalPraktek");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KodeJadwalPraktek",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
