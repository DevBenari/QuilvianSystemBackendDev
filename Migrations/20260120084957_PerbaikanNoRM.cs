using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class PerbaikanNoRM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pastikan index lama tidak bentrok (kalau pernah dibuat sebelumnya)
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_PdfPasienBaru_NoRekamMedis"";
            ");

            // Partial unique index: unik hanya untuk data yang tidak delete
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_PdfPasienBaru_NoRekamMedis""
                ON public.""PdfPasienBaru"" (""NoRekamMedis"")
                WHERE ""IsDelete"" IS DISTINCT FROM TRUE;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_PdfPasienBaru_NoRekamMedis"";
            ");
        }
    }
}
