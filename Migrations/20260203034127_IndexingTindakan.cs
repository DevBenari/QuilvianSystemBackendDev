using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class IndexingTindakan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Enable extension
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // 2) Trigram indexes for MstTindakan (sesuai entity: [Table(""MstTindakan"", Schema=""public"")])
            migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_msttindakan_kodetindakan_trgm
            ON public.""MstTindakan""
            USING gin (lower(""KodeTindakan"") gin_trgm_ops);
        ");

            migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_msttindakan_namatindakan_trgm
            ON public.""MstTindakan""
            USING gin (lower(""NamaTindakan"") gin_trgm_ops);
        ");

            // 3) Trigram index for Poliklinik name
            // NOTE: SESUAIKAN nama tabel & kolom jika berbeda di database kamu.
            // Contoh yang umum: public."Poliklinik" kolom "NamaPoliklinik"
            migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_poliklinik_namapoliklinik_trgm
            ON public.""MstPoliklinik""
            USING gin (lower(""NamaPoliklinik"") gin_trgm_ops);
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes (IF EXISTS aman untuk rerun/rollback sebagian)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS public.ix_mstpoliklinik_namapoliklinik_trgm;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS public.ix_msttindakan_namatindakan_trgm;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS public.ix_msttindakan_kodetindakan_trgm;");

            // Optional: drop extension (biasanya saya biarkan, tapi kalau mau bersih)
            migrationBuilder.Sql(@"DROP EXTENSION IF EXISTS pg_trgm;");
        }
    }
}
