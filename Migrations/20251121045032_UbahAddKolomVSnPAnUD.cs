using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahAddKolomVSnPAnUD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IndicatorScoreId",
                table: "UlkusDebituss",
                newName: "IndicatorPengkajianId");

            migrationBuilder.AddColumn<decimal>(
                name: "BBKering",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BBPostHD",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BBPreHD",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegularNadi",
                schema: "public",
                table: "MstVitalSign",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenambahanBBHD",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenguranganBBHD",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolaNapas",
                schema: "public",
                table: "MstVitalSign",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Respirasi",
                schema: "public",
                table: "MstVitalSign",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ekstremitas",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsKonjungtiva",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanUmum",
                schema: "public",
                table: "MstPainAssessment",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BBKering",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "BBPostHD",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "BBPreHD",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "IsRegularNadi",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "PenambahanBBHD",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "PenguranganBBHD",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "PolaNapas",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "Respirasi",
                schema: "public",
                table: "MstVitalSign");

            migrationBuilder.DropColumn(
                name: "Ekstremitas",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "IsKonjungtiva",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanUmum",
                schema: "public",
                table: "MstPainAssessment");

            migrationBuilder.RenameColumn(
                name: "IndicatorPengkajianId",
                table: "UlkusDebituss",
                newName: "IndicatorScoreId");
        }
    }
}
