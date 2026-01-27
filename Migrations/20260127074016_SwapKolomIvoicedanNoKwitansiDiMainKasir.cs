using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class SwapKolomIvoicedanNoKwitansiDiMainKasir : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InvoiceBilling",
                schema: "public",
                table: "MainKasirDetail",
                newName: "NoKwitansi");

            migrationBuilder.RenameColumn(
                name: "NoKwitansi",
                schema: "public",
                table: "MainKasir",
                newName: "InvoiceBilling");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NoKwitansi",
                schema: "public",
                table: "MainKasirDetail",
                newName: "InvoiceBilling");

            migrationBuilder.RenameColumn(
                name: "InvoiceBilling",
                schema: "public",
                table: "MainKasir",
                newName: "NoKwitansi");
        }
    }
}
