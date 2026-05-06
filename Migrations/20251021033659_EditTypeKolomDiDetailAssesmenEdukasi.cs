using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditTypeKolomDiDetailAssesmenEdukasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Ubah nilai kosong atau tidak valid jadi NULL dulu
            migrationBuilder.Sql(@"
                UPDATE ""AssesmentEdukasiDetails""
                SET ""TopikEdukasi"" = NULL
                WHERE ""TopikEdukasi"" IS NOT NULL 
                AND ""TopikEdukasi"" !~* '^[0-9a-fA-F-]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$';
            ");

            // 2️⃣ Alter kolom dengan USING agar PostgreSQL tahu cara konversinya
            migrationBuilder.Sql(@"
                ALTER TABLE ""AssesmentEdukasiDetails""
                ALTER COLUMN ""TopikEdukasi"" TYPE uuid
                USING NULLIF(""TopikEdukasi"", '')::uuid;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Kembalikan tipe kolom ke text jika rollback
            migrationBuilder.AlterColumn<string>(
                name: "TopikEdukasi",
                table: "AssesmentEdukasiDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
