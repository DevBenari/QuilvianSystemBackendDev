using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomLabBookingDetailSMId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cast uuid → uuid[] secara manual
            migrationBuilder.Sql(@"
                ALTER TABLE ""LabBookingDetails""
                ALTER COLUMN ""SpecimenMethodId"" TYPE uuid[]
                USING CASE 
                        WHEN ""SpecimenMethodId"" IS NULL 
                        THEN NULL 
                        ELSE ARRAY[""SpecimenMethodId""] 
                     END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Kembalikan array → single uuid
            migrationBuilder.Sql(@"
                ALTER TABLE ""LabBookingDetails""
                ALTER COLUMN ""SpecimenMethodId"" TYPE uuid
                USING CASE 
                        WHEN ""SpecimenMethodId"" IS NULL 
                        THEN NULL 
                        ELSE ""SpecimenMethodId""[1]
                     END;
            ");
        }
    }
}
