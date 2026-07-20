using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class RenameNamaTabelManual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrupCOAId",
                schema: "public",
                table: "Fin_MasterCoa");

            migrationBuilder.DropColumn(
                name: "TipeAkunCOAId",
                schema: "public",
                table: "Fin_MasterCoa");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupCOAId",
                schema: "public",
                table: "Fin_MasterCoa",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipeAkunCOAId",
                schema: "public",
                table: "Fin_MasterCoa",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
