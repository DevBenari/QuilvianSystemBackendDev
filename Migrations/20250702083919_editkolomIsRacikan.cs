using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editkolomIsRacikan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL untuk mengubah kolom string ke boolean secara aman
            migrationBuilder.Sql(
                @"ALTER TABLE ""MstResepDetail"" 
                  ALTER COLUMN ""IsRacikan"" 
                  TYPE boolean 
                  USING (CASE 
                      WHEN ""IsRacikan"" ILIKE 'yes' THEN TRUE
                      WHEN ""IsRacikan"" ILIKE 'true' THEN TRUE
                      WHEN ""IsRacikan"" = '1' THEN TRUE
                      WHEN ""IsRacikan"" ILIKE 'no' THEN FALSE
                      WHEN ""IsRacikan"" ILIKE 'false' THEN FALSE
                      WHEN ""IsRacikan"" = '0' THEN FALSE
                      ELSE NULL 
                  END);"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Kembalikan ke text jika dibatalkan
            migrationBuilder.AlterColumn<string>(
                name: "IsRacikan",
                schema: "public",
                table: "MstResepDetail",
                type: "text",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }
    }
}
