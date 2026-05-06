using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class HapusKolomDiskonDiKasirDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiskonId",
                schema: "public",
                table: "MainKasirDetail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiskonId",
                schema: "public",
                table: "MainKasirDetail",
                type: "uuid",
                nullable: true);
        }
    }
}
