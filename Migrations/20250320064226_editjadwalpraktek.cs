using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editjadwalpraktek : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
