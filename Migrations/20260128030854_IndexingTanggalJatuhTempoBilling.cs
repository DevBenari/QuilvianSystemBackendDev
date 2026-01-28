using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class IndexingTanggalJatuhTempoBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // 1) Backfill existing data (PostgreSQL)
            // Set jatuh tempo = DATE(TanggalInvoice) + 90 hari
            migrationBuilder.Sql(@"
                UPDATE ""Billing""
                SET ""TanggalJatuhTempo"" = (DATE(""TanggalInvoice"") + INTERVAL '90 days')
                WHERE ""TanggalInvoice"" IS NOT NULL
                  AND ""TanggalJatuhTempo"" IS NULL;
            ");

            // 3) Add index
            migrationBuilder.CreateIndex(
                name: "IX_Billing_TanggalJatuhTempo",
                table: "Billing",
                column: "TanggalJatuhTempo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Billing_TanggalJatuhTempo",
                table: "Billing");

        }
    }
}
