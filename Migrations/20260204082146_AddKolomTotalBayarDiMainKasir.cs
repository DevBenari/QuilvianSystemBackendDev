using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTotalBayarDiMainKasir : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalSebelumAsuransi",
                schema: "public",
                table: "MainKasir",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSebelumPajak",
                schema: "public",
                table: "MainKasir",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSetelahAsuransi",
                schema: "public",
                table: "MainKasir",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSebelumAsuransi",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "TotalSebelumPajak",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "TotalSetelahAsuransi",
                schema: "public",
                table: "MainKasir");
        }
    }
}
