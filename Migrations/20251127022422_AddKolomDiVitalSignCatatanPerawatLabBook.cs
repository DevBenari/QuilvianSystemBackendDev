using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiVitalSignCatatanPerawatLabBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TindakanId",
                table: "CatatanPerawats",
                newName: "PemeriksaanLabId");

            migrationBuilder.AddColumn<Guid>(
                name: "PengkajianScoreId",
                schema: "public",
                table: "MstVitalSign",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreGizi",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnjuranDiet",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilPenunjangLab",
                table: "LabBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TindakLanjut",
                table: "LabBookings",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PengkajianScoreId",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "ScoreGizi",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "AnjuranDiet",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "HasilPenunjangLab",
                table: "LabBookings");

            migrationBuilder.DropColumn(
                name: "TindakLanjut",
                table: "LabBookings");

            migrationBuilder.RenameColumn(
                name: "PemeriksaanLabId",
                table: "CatatanPerawats",
                newName: "TindakanId");
        }
    }
}
