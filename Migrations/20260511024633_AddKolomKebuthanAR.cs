using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomKebuthanAR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipePembayaran",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "NoRegistrasi",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoBill",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusBilling",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoRegistrasi",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "NoBill",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "StatusBilling",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.AlterColumn<string>(
                name: "TipePembayaran",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
