using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewKolomDiLabookingNamaspecimen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSpecimenLayak",
                schema: "public",
                table: "LabBooking",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Specimen",
                schema: "public",
                table: "LabBooking",
                type: "text[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSpecimenLayak",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "Specimen",
                schema: "public",
                table: "LabBooking");
        }
    }
}
