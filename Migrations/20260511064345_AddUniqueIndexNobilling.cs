using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddUniqueIndexNobilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MainKasir_NoBill_Unique""
                ON public.""MainKasir"" (""NoBill"")
                WHERE ""NoBill"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_MainKasir_NoBill""
                ON public.""MainKasir"" (""NoBill"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_MainKasir_NoBill"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_MainKasir_NoBill_Unique"";
            ");
        }
    }
}
