using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class edittabledetailicd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SoapId",
                schema: "public",
                table: "MstDetailICD",
                newName: "KunjunganId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KunjunganId",
                schema: "public",
                table: "MstDetailICD",
                newName: "SoapId");
        }
    }
}
