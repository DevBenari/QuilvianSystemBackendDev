using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddSequenceNoRM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Buat sequence kalau belum ada
            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS public.no_rm_seq
                START WITH 1
                INCREMENT BY 1;
            ");

            // 2) Set nilai sequence dari NoRekamMedis terbesar (STRING format NN-NN-NN-NN)
            //    + hanya data aktif: IsDelete IS DISTINCT FROM TRUE
            migrationBuilder.Sql(@"
                DO $$
                DECLARE mx bigint;
                BEGIN
                  SELECT COALESCE(MAX(
                    split_part(""NoRekamMedis"", '-', 1)::int * 1000000 +
                    split_part(""NoRekamMedis"", '-', 2)::int * 10000 +
                    split_part(""NoRekamMedis"", '-', 3)::int * 100 +
                    split_part(""NoRekamMedis"", '-', 4)::int
                  ), 0)
                  INTO mx
                  FROM public.""PdfPasienBaru""
                  WHERE ""NoRekamMedis"" IS NOT NULL
                    AND ""NoRekamMedis"" ~ '^\d{2}-\d{2}-\d{2}-\d{2}$'
                    AND ""IsDelete"" IS DISTINCT FROM TRUE;

                  -- set last_value = mx => nextval() akan menghasilkan mx+1
                  PERFORM setval('public.no_rm_seq', mx);
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP SEQUENCE IF EXISTS public.no_rm_seq;");
        }
    }
}
