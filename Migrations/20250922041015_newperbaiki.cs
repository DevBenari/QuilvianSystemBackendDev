using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newperbaiki : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "JnsUser");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "JnsPembayaran");

            migrationBuilder.AddColumn<int>(
                name: "KodePembayaran",
                schema: "public",
                table: "JnsUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "JnsUser",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "public",
                table: "JnsPembayaranNominal",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "KodePembayaran",
                schema: "public",
                table: "JnsPembayaranNominal",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Set",
                schema: "public",
                table: "JnsPembayaran",
                type: "text",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "KodePembayaran",
                schema: "public",
                table: "JnsPembayaran",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "JnsPembayaran",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KodePembayaran",
                schema: "public",
                table: "JnsUser");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "JnsUser");

            migrationBuilder.DropColumn(
                name: "KodePembayaran",
                schema: "public",
                table: "JnsPembayaranNominal");

            migrationBuilder.DropColumn(
                name: "KodePembayaran",
                schema: "public",
                table: "JnsPembayaran");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "JnsPembayaran");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "JnsUser",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "public",
                table: "JnsPembayaranNominal",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Set",
                schema: "public",
                table: "JnsPembayaran",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "JnsPembayaran",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
