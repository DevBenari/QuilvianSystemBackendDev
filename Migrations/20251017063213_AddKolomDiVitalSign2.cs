using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiVitalSign2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AssesmentEdukasiDetail",
                table: "AssesmentEdukasiDetail");

            migrationBuilder.RenameTable(
                name: "AssesmentEdukasiDetail",
                newName: "AssesmentEdukasiDetails");

            migrationBuilder.AddColumn<string>(
                name: "FrekuensiMonitoring",
                schema: "public",
                table: "MstVitalSign",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasilMAP",
                schema: "public",
                table: "MstVitalSign",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MAP",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OksigenTambahan",
                schema: "public",
                table: "MstVitalSign",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenggunaanOksigen",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SkorEWS",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssesmentEdukasiDetails",
                table: "AssesmentEdukasiDetails",
                column: "DetailAsesmenEdukasiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AssesmentEdukasiDetails",
                table: "AssesmentEdukasiDetails");

            migrationBuilder.DropColumn(
                name: "FrekuensiMonitoring",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "HasilMAP",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "MAP",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "OksigenTambahan",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "PenggunaanOksigen",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "SkorEWS",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.RenameTable(
                name: "AssesmentEdukasiDetails",
                newName: "AssesmentEdukasiDetail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssesmentEdukasiDetail",
                table: "AssesmentEdukasiDetail",
                column: "DetailAsesmenEdukasiId");
        }
    }
}
