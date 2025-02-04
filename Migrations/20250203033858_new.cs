using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class @new : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlamatKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "HubunganKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KabupatenKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "KelurahanKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NamaAyah",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NamaIbu",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NamaSutri",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NomorKeluargaTerdekat",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NomorKtpSutri",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "NomorTeleponKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.RenameColumn(
                name: "NamaDepan",
                table: "AspNetUsers",
                newName: "NamaUser");

            migrationBuilder.RenameColumn(
                name: "NamaBelakang",
                table: "AspNetUsers",
                newName: "KodeUser");

            migrationBuilder.AddColumn<string>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IsOnline",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "NamaUser",
                table: "AspNetUsers",
                newName: "NamaDepan");

            migrationBuilder.RenameColumn(
                name: "KodeUser",
                table: "AspNetUsers",
                newName: "NamaBelakang");

            migrationBuilder.AddColumn<string>(
                name: "AlamatKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HubunganKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KabupatenKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KelurahanKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaAyah",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaIbu",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaSutri",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorKeluargaTerdekat",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorKtpSutri",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomorTeleponKeluarga",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
