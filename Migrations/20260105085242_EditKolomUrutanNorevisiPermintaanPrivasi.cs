using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditKolomUrutanNorevisiPermintaanPrivasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===============================================
            // 1) Bersihkan data tidak numeric (jadi NULL)
            // ===============================================
            migrationBuilder.Sql(@"
                UPDATE ""PermintaanPrivasis""
                SET ""Urutan"" = NULL
                WHERE ""Urutan"" IS NOT NULL
                  AND (trim(""Urutan"") = '' OR ""Urutan"" !~ '^[0-9]+(\.[0-9]+)?$');
            ");

            migrationBuilder.Sql(@"
                UPDATE ""PermintaanPrivasis""
                SET ""NoRevisi"" = NULL
                WHERE ""NoRevisi"" IS NOT NULL
                  AND (trim(""NoRevisi"") = '' OR ""NoRevisi"" !~ '^[0-9]+(\.[0-9]+)?$');
            ");

            // ===============================================
            // 2) Ubah tipe dengan USING
            // ===============================================
            migrationBuilder.Sql(@"
                ALTER TABLE ""PermintaanPrivasis""
                ALTER COLUMN ""Urutan"" TYPE numeric
                USING NULLIF(trim(""Urutan""), '')::numeric;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""PermintaanPrivasis""
                ALTER COLUMN ""NoRevisi"" TYPE numeric
                USING NULLIF(trim(""NoRevisi""), '')::numeric;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // balik lagi ke text
            migrationBuilder.Sql(@"
                ALTER TABLE ""PermintaanPrivasis""
                ALTER COLUMN ""Urutan"" TYPE text
                USING ""Urutan""::text;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""PermintaanPrivasis""
                ALTER COLUMN ""NoRevisi"" TYPE text
                USING ""NoRevisi""::text;
            ");
        }
    }
}
