using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class IndexingNoKwitansiDepositRanap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DepositRanaps_NoKwitansi",
                table: "DepositRanaps",
                column: "NoKwitansi",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepositRanaps_NoKwitansi",
                table: "DepositRanaps");
        }
    }
}
