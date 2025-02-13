using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UpdateNamaColumnMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JenisJabatan",
                schema: "dbo",
                table: "MstJabatan",
                newName: "NamaJabatan");

            migrationBuilder.RenameColumn(
                name: "JabatanKode",
                schema: "dbo",
                table: "MstJabatan",
                newName: "KodeJabatan");

            migrationBuilder.RenameColumn(
                name: "KdIdentitas",
                schema: "dbo",
                table: "MstIdentitas",
                newName: "KodeIdentitas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NamaJabatan",
                schema: "dbo",
                table: "MstJabatan",
                newName: "JenisJabatan");

            migrationBuilder.RenameColumn(
                name: "KodeJabatan",
                schema: "dbo",
                table: "MstJabatan",
                newName: "JabatanKode");

            migrationBuilder.RenameColumn(
                name: "KodeIdentitas",
                schema: "dbo",
                table: "MstIdentitas",
                newName: "KdIdentitas");
        }
    }
}
