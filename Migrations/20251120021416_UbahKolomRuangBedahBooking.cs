using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomRuangBedahBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE ""RuangBedahBookingDetails""
            ALTER COLUMN ""TindakanId"" TYPE uuid[]
            USING ARRAY[""TindakanId""::uuid];
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE ""RuangBedahBookingDetails""
            ALTER COLUMN ""TindakanId"" TYPE uuid
            USING (""TindakanId""[1]);
            ");
        }
    }
}
