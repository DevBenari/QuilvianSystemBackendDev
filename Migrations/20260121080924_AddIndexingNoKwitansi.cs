using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddIndexingNoKwitansi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 2) Unique index (NoKwitansi harus unik jika terisi)
            migrationBuilder.CreateIndex(
                name: "IX_MainKasir_NoKwitansi",
                schema: "public",
                table: "MainKasir",
                column: "NoKwitansi",
                unique: true,
                filter: "\"NoKwitansi\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MainKasir_NoKwitansi",
                schema: "public",
                table: "MainKasir");

        }
    }
}
