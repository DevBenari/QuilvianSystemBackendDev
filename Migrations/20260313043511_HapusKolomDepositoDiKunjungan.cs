using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class HapusKolomDepositoDiKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositRanap",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.RenameColumn(
                name: "NominalTransaksi",
                table: "DepositRanaps",
                newName: "NominalMasuk");


            migrationBuilder.AddColumn<decimal>(
                name: "NominalKeluar",
                table: "DepositRanaps",
                type: "numeric",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "NominalKeluar",
                table: "DepositRanaps");

            migrationBuilder.RenameColumn(
                name: "NominalMasuk",
                table: "DepositRanaps",
                newName: "NominalTransaksi");

            migrationBuilder.AddColumn<decimal>(
                name: "DepositRanap",
                schema: "public",
                table: "MstKunjungan",
                type: "numeric",
                nullable: true);

        }
    }
}
