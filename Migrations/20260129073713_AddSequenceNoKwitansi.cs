using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddSequenceNoKwitansi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Pastikan kolom NoKwitansi ada di MainKasirDetails
            migrationBuilder.Sql(@"
            ALTER TABLE public.""MainKasirDetail""
            ADD COLUMN IF NOT EXISTS ""NoKwitansi"" text;
        ");

            // 2) Buat tabel counter harian untuk kwitansi
            migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS public.""KwitansiSequences"" (
                ""KwitansiDate"" date PRIMARY KEY,
                ""LastSeq"" integer NOT NULL,
                ""UpdatedAt"" timestamptz NOT NULL
            );
        ");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // rollback index + table
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS public.""KwitansiSequences"";");



        }
    }
}
