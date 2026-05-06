using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditKolomPengawasanHarian : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PainAssesmentId",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "ResepId",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "VitalSignId",
                table: "PengawasanHarians");

            migrationBuilder.AddColumn<string>(
                name: "PainAssesment",
                table: "PengawasanHarians",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resep",
                table: "PengawasanHarians",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitalSign",
                table: "PengawasanHarians",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PainAssesment",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "Resep",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "VitalSign",
                table: "PengawasanHarians");

            migrationBuilder.AddColumn<Guid>(
                name: "PainAssesmentId",
                table: "PengawasanHarians",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResepId",
                table: "PengawasanHarians",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VitalSignId",
                table: "PengawasanHarians",
                type: "uuid",
                nullable: true);
        }
    }
}
