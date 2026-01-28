using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahkolomJadiListDiLaporanBedah : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guid (uuid) -> uuid[] : bungkus nilai lama jadi array 1 elemen
            migrationBuilder.Sql(@"
                ALTER TABLE ""LaporanBedahs""
                ALTER COLUMN ""TindakanId"" TYPE uuid[]
                USING CASE
                    WHEN ""TindakanId"" IS NULL THEN NULL
                    ELSE ARRAY[""TindakanId""]
                END;
            ");

            // text -> text[] : bungkus nilai lama jadi array 1 elemen
            migrationBuilder.Sql(@"
                ALTER TABLE ""LaporanBedahs""
                ALTER COLUMN ""DiagnosaPraOp"" TYPE text[]
                USING CASE
                    WHEN ""DiagnosaPraOp"" IS NULL THEN NULL
                    ELSE ARRAY[""DiagnosaPraOp""]
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""LaporanBedahs""
                ALTER COLUMN ""DiagnosaPostOp"" TYPE text[]
                USING CASE
                    WHEN ""DiagnosaPostOp"" IS NULL THEN NULL
                    ELSE ARRAY[""DiagnosaPostOp""]
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // uuid[] -> uuid : ambil elemen pertama
            migrationBuilder.Sql(@"
                ALTER TABLE ""LaporanBedahs""
                ALTER COLUMN ""TindakanId"" TYPE uuid
                USING CASE
                    WHEN ""TindakanId"" IS NULL OR array_length(""TindakanId"", 1) IS NULL THEN NULL
                    ELSE ""TindakanId""[1]
                END;
            ");

            // text[] -> text : ambil elemen pertama
            migrationBuilder.Sql(@"
                ALTER TABLE ""LaporanBedahs""
                ALTER COLUMN ""DiagnosaPraOp"" TYPE text
                USING CASE
                    WHEN ""DiagnosaPraOp"" IS NULL OR array_length(""DiagnosaPraOp"", 1) IS NULL THEN NULL
                    ELSE ""DiagnosaPraOp""[1]
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""LaporanBedahs""
                ALTER COLUMN ""DiagnosaPostOp"" TYPE text
                USING CASE
                    WHEN ""DiagnosaPostOp"" IS NULL OR array_length(""DiagnosaPostOp"", 1) IS NULL THEN NULL
                    ELSE ""DiagnosaPostOp""[1]
                END;
            ");
        }
    }
}
