using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editjp3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek");
        }
    }
}
