using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class fixkun : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TindakanId",
                schema: "public",
                table: "MstKunjungan");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TindakanId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);
        }
    }
}
