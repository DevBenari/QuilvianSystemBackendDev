using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class changeModelAgama : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JenisAgama",
                schema: "dbo",
                table: "MstAgama",
                newName: "NamaAgama");

            migrationBuilder.RenameColumn(
                name: "AgamaKode",
                schema: "dbo",
                table: "MstAgama",
                newName: "KodeAgama");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NamaAgama",
                schema: "dbo",
                table: "MstAgama",
                newName: "JenisAgama");

            migrationBuilder.RenameColumn(
                name: "KodeAgama",
                schema: "dbo",
                table: "MstAgama",
                newName: "AgamaKode");
        }
    }
}
