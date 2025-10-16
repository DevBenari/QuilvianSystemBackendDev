using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPengawasanHarian : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PainAssesment",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PainAssesment",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "ResepId",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "VitalSignId",
                table: "PengawasanHarians");
        }
    }
}
