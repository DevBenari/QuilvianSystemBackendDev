using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class RevisiTableCatatanPemulihanDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_catatanPemulihanDetails",
                table: "catatanPemulihanDetails");

            migrationBuilder.RenameTable(
                name: "catatanPemulihanDetails",
                newName: "CatatanPemulihanDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatatanPemulihanDetails",
                table: "CatatanPemulihanDetails",
                column: "DetailCatPemulihanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CatatanPemulihanDetails",
                table: "CatatanPemulihanDetails");

            migrationBuilder.RenameTable(
                name: "CatatanPemulihanDetails",
                newName: "catatanPemulihanDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_catatanPemulihanDetails",
                table: "catatanPemulihanDetails",
                column: "DetailCatPemulihanId");
        }
    }
}
