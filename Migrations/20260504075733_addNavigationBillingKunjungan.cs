using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addNavigationBillingKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFoC",
                table: "TindakanKunjungans",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billing_KunjunganId",
                schema: "public",
                table: "Billing",
                column: "KunjunganId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_MstKunjungan_KunjunganId",
                schema: "public",
                table: "Billing",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billing_MstKunjungan_KunjunganId",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropIndex(
                name: "IX_Billing_KunjunganId",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "IsFoC",
                table: "TindakanKunjungans");
        }
    }
}
