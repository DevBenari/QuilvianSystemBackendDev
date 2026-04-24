using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPainAssesmentRiwaytPen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIGD",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KondisiMasukIGD",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MasukIGD",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiwayatPenyakit",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIGD",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "KondisiMasukIGD",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "MasukIGD",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "RiwayatPenyakit",
                schema: "public",
                table: "MstPainAssessment");
        }
    }
}
