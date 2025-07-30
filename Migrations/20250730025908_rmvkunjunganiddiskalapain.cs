using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class rmvkunjunganiddiskalapain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KunjunganId",
                schema: "public",
                table: "MstSkalaPain");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KunjunganId",
                schema: "public",
                table: "MstSkalaPain",
                type: "uuid",
                nullable: true);
        }
    }
}
