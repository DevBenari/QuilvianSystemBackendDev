using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class fixpdfpasein : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoRekamMedisLama",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.AlterColumn<string>(
                name: "NoTelepon3",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoTelepon2",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoTelepon1",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NoTelepon3",
                schema: "public",
                table: "PdfPasienBaru",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NoTelepon2",
                schema: "public",
                table: "PdfPasienBaru",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NoTelepon1",
                schema: "public",
                table: "PdfPasienBaru",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoRekamMedisLama",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);
        }
    }
}
