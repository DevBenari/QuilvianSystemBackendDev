using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddQueryIndexingNoKwitansi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UNIQUE index normal (aman kalau kamu tidak pakai soft delete untuk NoKwitansi)
            // migrationBuilder.CreateIndex(
            //     name: "IX_MainKasirDetails_NoKwitansi",
            //     table: "MainKasirDetails",
            //     column: "NoKwitansi",
            //     unique: true);

            // Lebih aman: UNIQUE partial index (PostgreSQL) -> abaikan row yang IsDelete = true
            migrationBuilder.Sql(@"
            CREATE UNIQUE INDEX IF NOT EXISTS ""UX_MainKasirDetail_NoKwitansi_Active""
            ON ""MainKasirDetail"" (""NoKwitansi"")
            WHERE ""NoKwitansi"" IS NOT NULL
              AND (""IsDelete"" = FALSE OR ""IsDelete"" IS NULL);
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP INDEX IF EXISTS ""UX_MainKasirDetail_NoKwitansi_Active"";
        ");
        }
    }
}
