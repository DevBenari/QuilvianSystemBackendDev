using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addparamranapid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                schema: "public",
                table: "MstVitalSign",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                schema: "public",
                table: "MstSOAP",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                schema: "public",
                table: "MstPainAssessment",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RanapId",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "RanapId",
                schema: "public",
                table: "MstSOAP");

            migrationBuilder.DropColumn(
                name: "RanapId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "RanapId",
                schema: "public",
                table: "MstPainAssessment");
        }
    }
}
