using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class tambahkolomruang : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Lokasi",
                schema: "public",
                table: "MstPoliklinik",
                newName: "Ruang");

            migrationBuilder.AddColumn<string>(
                name: "KodeAntreanPoli",
                schema: "public",
                table: "MstPoliklinik",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KodeAntreanPoli",
                schema: "public",
                table: "MstPoliklinik");

            migrationBuilder.RenameColumn(
                name: "Ruang",
                schema: "public",
                table: "MstPoliklinik",
                newName: "Lokasi");
        }
    }
}
