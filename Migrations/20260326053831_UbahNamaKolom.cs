using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahNamaKolom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoveragePercentage",
                schema: "public",
                table: "Billing");

            migrationBuilder.RenameColumn(
                name: "NoWali3",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NoWali1");

            migrationBuilder.RenameColumn(
                name: "NamaWali3",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NamaWali1");

            migrationBuilder.AddColumn<decimal>(
                name: "CoveragePercentage",
                schema: "public",
                table: "MstAsuransi",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoveragePercentage",
                schema: "public",
                table: "MstAsuransi");

            migrationBuilder.RenameColumn(
                name: "NoWali1",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NoWali3");

            migrationBuilder.RenameColumn(
                name: "NamaWali1",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NamaWali3");

            migrationBuilder.AddColumn<decimal>(
                name: "CoveragePercentage",
                schema: "public",
                table: "Billing",
                type: "numeric",
                nullable: true);
        }
    }
}
