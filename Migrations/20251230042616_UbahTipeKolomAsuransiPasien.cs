using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahTipeKolomAsuransiPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PasienId: text -> uuid
            migrationBuilder.Sql(@"
            ALTER TABLE public.""MstAsuransiPasien""
            ALTER COLUMN ""PasienId"" TYPE uuid
            USING ""PasienId""::uuid;
        ");

            // AsuransiId: text -> uuid
            migrationBuilder.Sql(@"
            ALTER TABLE public.""MstAsuransiPasien""
            ALTER COLUMN ""AsuransiId"" TYPE uuid
            USING ""AsuransiId""::uuid;
        ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE public.""MstAsuransiPasien""
            ALTER COLUMN ""PasienId"" TYPE text;
        ");

            migrationBuilder.Sql(@"
            ALTER TABLE public.""MstAsuransiPasien""
            ALTER COLUMN ""AsuransiId"" TYPE text;
        ");
        }
    }

}
