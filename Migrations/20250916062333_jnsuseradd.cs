using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class jnsuseradd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nomor",
                schema: "public",
                table: "Adm_JenisUser",
                newName: "Tlp");

            migrationBuilder.AddColumn<string>(
                name: "No",
                schema: "public",
                table: "Adm_JenisUser",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "No",
                schema: "public",
                table: "Adm_JenisUser");

            migrationBuilder.RenameColumn(
                name: "Tlp",
                schema: "public",
                table: "Adm_JenisUser",
                newName: "Nomor");
        }
    }
}
