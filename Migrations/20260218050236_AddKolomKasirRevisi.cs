using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomKasirRevisi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalSetelahAsuransi",
                schema: "public",
                table: "MainKasir",
                newName: "TotalPembayaran");

            migrationBuilder.RenameColumn(
                name: "TotalSebelumPajak",
                schema: "public",
                table: "MainKasir",
                newName: "SubTotalMandiri");

            migrationBuilder.RenameColumn(
                name: "TotalSebelumAsuransi",
                schema: "public",
                table: "MainKasir",
                newName: "SubTotalAsuransi");

            migrationBuilder.AddColumn<decimal>(
                name: "Deposito",
                schema: "public",
                table: "MainKasir",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deposito",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.RenameColumn(
                name: "TotalPembayaran",
                schema: "public",
                table: "MainKasir",
                newName: "TotalSetelahAsuransi");

            migrationBuilder.RenameColumn(
                name: "SubTotalMandiri",
                schema: "public",
                table: "MainKasir",
                newName: "TotalSebelumPajak");

            migrationBuilder.RenameColumn(
                name: "SubTotalAsuransi",
                schema: "public",
                table: "MainKasir",
                newName: "TotalSebelumAsuransi");
        }
    }
}
