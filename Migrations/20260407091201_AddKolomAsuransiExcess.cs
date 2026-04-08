using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomAsuransiExcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExcess",
                schema: "public",
                table: "MstAsuransiPasien",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "IsExcess",
                schema: "public",
                table: "MstAsuransiPasien");
        }
    }
}
