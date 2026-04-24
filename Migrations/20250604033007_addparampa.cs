using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addparampa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HasilResikoJatuh",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAsiEksklusif",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAtaksia",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBCGimunisasi",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCampakImunisasi",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDPTImunisasi",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHepatitisBImunisasi",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInteraksiSosial",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMotorikAktif",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPolioImunisasi",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPosturalInstability",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsResponsAuditori",
                schema: "public",
                table: "MstPainAssessment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusMpasi",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasilResikoJatuh",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsAsiEksklusif",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsAtaksia",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsBCGimunisasi",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsCampakImunisasi",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsDPTImunisasi",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsHepatitisBImunisasi",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsInteraksiSosial",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsMotorikAktif",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsPolioImunisasi",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsPosturalInstability",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsResponsAuditori",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "StatusMpasi",
                schema: "public",
                table: "MstPainAssessment");
        }
    }
}
