using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKaryawanIdJadiGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE public.""PdfPasienBaru""
                SET ""KaryawanId"" = NULL
                WHERE ""KaryawanId"" IS NOT NULL
                  AND (
                        trim(""KaryawanId"") = ''
                        OR trim(""KaryawanId"") !~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                  );
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE public.""PdfPasienBaru""
                ALTER COLUMN ""KaryawanId"" DROP DEFAULT;

                ALTER TABLE public.""PdfPasienBaru""
                ALTER COLUMN ""KaryawanId"" TYPE uuid
                USING NULLIF(trim(""KaryawanId""), '')::uuid;
            ");


            migrationBuilder.CreateIndex(
                name: "IX_MainKasir_KunjunganId",
                schema: "public",
                table: "MainKasir",
                column: "KunjunganId");

            migrationBuilder.AddForeignKey(
                name: "FK_MainKasir_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MainKasir",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MainKasir_MstKunjungan_KunjunganId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropIndex(
                name: "IX_MainKasir_KunjunganId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.Sql(@"
                ALTER TABLE public.""PdfPasienBaru""
                ALTER COLUMN ""KaryawanId"" TYPE text
                USING ""KaryawanId""::text;
            ");
        }
    }
}
