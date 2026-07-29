using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavUserActiveDiDokter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstDokter_UserActiveId",
                schema: "public",
                table: "MstDokter",
                column: "UserActiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokter_MstUserActive_UserActiveId",
                schema: "public",
                table: "MstDokter",
                column: "UserActiveId",
                principalSchema: "public",
                principalTable: "MstUserActive",
                principalColumn: "UserActiveId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDokter_MstUserActive_UserActiveId",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.DropIndex(
                name: "IX_MstDokter_UserActiveId",
                schema: "public",
                table: "MstDokter");
        }
    }
}
