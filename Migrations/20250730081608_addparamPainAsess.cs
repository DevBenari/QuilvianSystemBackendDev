using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addparamPainAsess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentMedication",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RPD",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RPS",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentMedication",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "RPD",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "RPS",
                schema: "public",
                table: "MstPainAssessment");
        }
    }
}
