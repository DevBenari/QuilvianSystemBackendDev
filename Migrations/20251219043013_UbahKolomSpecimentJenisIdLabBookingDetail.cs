using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomSpecimentJenisIdLabBookingDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ubah tipe kolom uuid -> uuid[] dengan konversi value lama menjadi array 1 elemen
            migrationBuilder.Sql(@"
        ALTER TABLE ""LabBookingDetails""
        ALTER COLUMN ""SpecimenJenisId"" TYPE uuid[]
        USING CASE
            WHEN ""SpecimenJenisId"" IS NULL THEN NULL
            ELSE ARRAY[""SpecimenJenisId""]
        END;
    ");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ubah tipe kolom uuid[] -> uuid (ambil elemen pertama)
            migrationBuilder.Sql(@"
        ALTER TABLE ""LabBookingDetails""
        ALTER COLUMN ""SpecimenJenisId"" TYPE uuid
        USING CASE
            WHEN ""SpecimenJenisId"" IS NULL THEN NULL
            WHEN array_length(""SpecimenJenisId"", 1) IS NULL THEN NULL
            ELSE ""SpecimenJenisId""[1]
        END;
    ");
        }

    }
}
