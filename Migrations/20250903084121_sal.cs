using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class sal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Jobtype",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryRange",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Jobtype",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");

            migrationBuilder.DropColumn(
                name: "SalaryRange",
                schema: "public",
                table: "Hrd_PengajuanRekrutmen");
        }
    }
}
