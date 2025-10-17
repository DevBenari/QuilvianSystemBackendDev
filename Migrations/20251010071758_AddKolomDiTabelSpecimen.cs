using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiTabelSpecimen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpecimenId",
                table: "SpecimenMethods",
                newName: "SpecimenJenisId");

            migrationBuilder.AddColumn<Guid>(
                name: "SpecimenJenisId",
                table: "LabBookingDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecimenMethodId",
                table: "LabBookingDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecimenTestId",
                table: "LabBookingDetails",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecimenJenisId",
                table: "LabBookingDetails");

            migrationBuilder.DropColumn(
                name: "SpecimenMethodId",
                table: "LabBookingDetails");

            migrationBuilder.DropColumn(
                name: "SpecimenTestId",
                table: "LabBookingDetails");

            migrationBuilder.RenameColumn(
                name: "SpecimenJenisId",
                table: "SpecimenMethods",
                newName: "SpecimenId");
        }
    }
}
