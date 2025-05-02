using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class obatEnd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BentukObat",
                schema: "public",
                table: "MstObat",
                newName: "BentukObatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BentukObatId",
                schema: "public",
                table: "MstObat",
                newName: "BentukObat");
        }
    }
}
