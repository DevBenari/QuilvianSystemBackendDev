using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiDetailResep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlasanReturn",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DikembalikanOleh",
                schema: "public",
                table: "MstResepDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturn",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyReturn",
                schema: "public",
                table: "MstResepDetail",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlasanReturn",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "DikembalikanOleh",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "IsReturn",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "QtyReturn",
                schema: "public",
                table: "MstResepDetail");
        }
    }
}
