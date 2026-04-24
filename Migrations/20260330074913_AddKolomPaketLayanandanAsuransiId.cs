using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPaketLayanandanAsuransiId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaketLayananDiskonId",
                schema: "public",
                table: "MainKasir",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "Billing",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCovered",
                schema: "public",
                table: "Billing",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaketLayananDiskonId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "AsuransiId",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "IsCovered",
                schema: "public",
                table: "Billing");
        }
    }
}
