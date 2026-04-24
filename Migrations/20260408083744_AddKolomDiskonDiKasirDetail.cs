using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiskonDiKasirDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiskonId",
                schema: "public",
                table: "MainKasirDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeDiskonDokter",
                schema: "public",
                table: "MainKasirDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueDiskonDokter",
                schema: "public",
                table: "MainKasirDetail",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiskonId",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "TipeDiskonDokter",
                schema: "public",
                table: "MainKasirDetail");

            migrationBuilder.DropColumn(
                name: "ValueDiskonDokter",
                schema: "public",
                table: "MainKasirDetail");
        }
    }
}
