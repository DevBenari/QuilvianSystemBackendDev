using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahStringToGuidSupplierIdPO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.""Fin_PurchaseOrder""
                ALTER COLUMN ""SupplierId"" TYPE uuid
                USING CASE
                    WHEN ""SupplierId"" IS NULL THEN NULL
                    WHEN btrim(""SupplierId"") = '' THEN NULL
                    WHEN btrim(""SupplierId"") ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                        THEN btrim(""SupplierId"")::uuid
                    ELSE NULL
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.""Fin_PurchaseOrder""
                ALTER COLUMN ""SupplierId"" TYPE text
                USING ""SupplierId""::text;
            ");
        }
    }
}