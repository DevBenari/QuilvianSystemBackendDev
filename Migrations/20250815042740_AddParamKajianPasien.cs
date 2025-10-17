using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamKajianPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentMedicationId",
                schema: "public",
                table: "KajianPasien",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KajianUtamaPengkajian",
                schema: "public",
                table: "KajianPasien",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentMedicationId",
                schema: "public",
                table: "KajianPasien");

            migrationBuilder.DropColumn(
                name: "KajianUtamaPengkajian",
                schema: "public",
                table: "KajianPasien");
        }
    }
}
