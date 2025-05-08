using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class edittabelkunjgan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JumlahKunjungan",
                schema: "public",
                table: "MstKunjungan",
                newName: "JenisKunjungan");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JenisKunjungan",
                schema: "public",
                table: "MstKunjungan",
                newName: "JumlahKunjungan");
        }
    }
}
