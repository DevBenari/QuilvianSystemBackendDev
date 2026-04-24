using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomPengkajianScoreIDJadiList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Rename kolom lama
            migrationBuilder.RenameColumn(
                name: "PengkajianScoreId",
                schema: "public",
                table: "MstVitalSign",
                newName: "PengkajianScoreId_Old");

            // 2️⃣ Tambah kolom baru sebagai array UUID
            migrationBuilder.AddColumn<Guid[]>(
                name: "PengkajianScoreId",
                schema: "public",
                table: "MstVitalSign",
                type: "uuid[]",
                nullable: true);

            // 3️⃣ Migrasi data lama (jadikan array)
            migrationBuilder.Sql(@"
                UPDATE public.""MstVitalSign""
                SET ""PengkajianScoreId"" = 
                    CASE 
                        WHEN ""PengkajianScoreId_Old"" IS NOT NULL 
                        THEN ARRAY[""PengkajianScoreId_Old""]::uuid[]
                        ELSE NULL
                    END;
            ");

            // 4️⃣ Hapus kolom lama
            migrationBuilder.DropColumn(
                name: "PengkajianScoreId_Old",
                schema: "public",
                table: "MstVitalSign");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Tambah kembali kolom lama tipe uuid
            migrationBuilder.AddColumn<Guid>(
                name: "PengkajianScoreId_Old",
                schema: "public",
                table: "MstVitalSign",
                type: "uuid",
                nullable: true);

            // 2️⃣ Convert array → ambil elemen pertama (jika ada)
            migrationBuilder.Sql(@"
                UPDATE public.""MstVitalSign""
                SET ""PengkajianScoreId_Old"" = 
                    CASE
                        WHEN ""PengkajianScoreId"" IS NOT NULL 
                             AND array_length(""PengkajianScoreId"", 1) > 0
                        THEN ""PengkajianScoreId""[1]
                        ELSE NULL
                    END;
            ");

            // 3️⃣ Drop kolom array baru
            migrationBuilder.DropColumn(
                name: "PengkajianScoreId",
                schema: "public",
                table: "MstVitalSign");

            // 4️⃣ Rename kembali ke nama aslinya
            migrationBuilder.RenameColumn(
                name: "PengkajianScoreId_Old",
                schema: "public",
                table: "MstVitalSign",
                newName: "PengkajianScoreId");
        }
    }
}
