using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPeralatanIdDanKelasId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PeralatanId",
                schema: "public",
                table: "MstTarifKelas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KelasId",
                table: "AlatPemakaianDetails",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeralatanId",
                schema: "public",
                table: "MstTarifKelas");

            migrationBuilder.DropColumn(
                name: "KelasId",
                table: "AlatPemakaianDetails");
        }
    }
}
