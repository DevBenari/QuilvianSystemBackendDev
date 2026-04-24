using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class HapusKolomDiPengawasanHarian : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PainAssessmentId",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "ResepId",
                table: "PengawasanHarians");

            migrationBuilder.DropColumn(
                name: "VitalSignId",
                table: "PengawasanHarians");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PainAssessmentId",
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
