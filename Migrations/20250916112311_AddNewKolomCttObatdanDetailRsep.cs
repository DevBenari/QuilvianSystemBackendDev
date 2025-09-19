using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewKolomCttObatdanDetailRsep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StatusDiberikanPasien",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RacikanId",
                table: "CttPemberianObats",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusDiberikanPasien",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "RacikanId",
                table: "CttPemberianObats");
        }
    }
}
