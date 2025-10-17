using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditParamSDKI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SDKIEtiologiId",
                schema: "public",
                table: "SDKITeraupetik",
                newName: "SDKIDiagnosaId");

            migrationBuilder.RenameColumn(
                name: "SDKIEtiologiId",
                schema: "public",
                table: "SDKIKolaborasi",
                newName: "SDKIDiagnosaId");

            migrationBuilder.RenameColumn(
                name: "SDKIEtiologiId",
                schema: "public",
                table: "SDKIEdukasi",
                newName: "SDKIDiagnosaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SDKIDiagnosaId",
                schema: "public",
                table: "SDKITeraupetik",
                newName: "SDKIEtiologiId");

            migrationBuilder.RenameColumn(
                name: "SDKIDiagnosaId",
                schema: "public",
                table: "SDKIKolaborasi",
                newName: "SDKIEtiologiId");

            migrationBuilder.RenameColumn(
                name: "SDKIDiagnosaId",
                schema: "public",
                table: "SDKIEdukasi",
                newName: "SDKIEtiologiId");
        }
    }
}
