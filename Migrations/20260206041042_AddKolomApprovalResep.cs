using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomApprovalResep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PathTTDDokter",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PetugasFarmasiId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TTDPetugasFarmasiId",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathTTDDokter",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "PetugasFarmasiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "TTDPetugasFarmasiId",
                schema: "public",
                table: "MstResep");
        }
    }
}
