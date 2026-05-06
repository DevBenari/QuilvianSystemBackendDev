using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPositionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "public",
                table: "MstDepartement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionName",
                schema: "public",
                table: "MstDepartement",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionCode",
                schema: "public",
                table: "MstDepartement");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "public",
                table: "MstDepartement");

            migrationBuilder.DropColumn(
                name: "PositionName",
                schema: "public",
                table: "MstDepartement");
        }
    }
}
