using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePemakaianAlat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCTTPasienIGD",
                schema: "public",
                table: "MstKunjungan",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTriage",
                schema: "public",
                table: "MstKunjungan",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCTTPasienIGD",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "IsTriage",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
