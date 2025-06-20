using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addracikanid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RacikanId",
                schema: "public",
                table: "MstResepDetail",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRacikan",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "RacikanId",
                schema: "public",
                table: "MstResepDetail");
        }
    }
}
