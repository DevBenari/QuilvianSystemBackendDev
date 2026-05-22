using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomResepTebusDikasir : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResepTebusId",
                schema: "public",
                table: "MainKasir",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResepTebusId",
                schema: "public",
                table: "Billing",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResepTebusId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "ResepTebusId",
                schema: "public",
                table: "Billing");
        }
    }
}
