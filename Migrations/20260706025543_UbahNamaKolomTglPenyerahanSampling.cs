using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahNamaKolomTglPenyerahanSampling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TglPenyerahanSampling",
                schema: "public",
                table: "LabBooking",
                newName: "TglSampling");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameColumn(
                name: "TglSampling",
                schema: "public",
                table: "LabBooking",
                newName: "TglPenyerahanSampling");
        }
    }
}
