using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class GantiNamaPoli : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameColumn(
                name: "PoliId",
                schema: "public",
                table: "MstTindakanPoli",
                newName: "PoliklinikId");

        
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
    
            migrationBuilder.RenameColumn(
                name: "PoliklinikId",
                schema: "public",
                table: "MstTindakanPoli",
                newName: "PoliId");

  
        }
    }
}
