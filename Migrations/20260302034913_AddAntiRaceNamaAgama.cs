using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddAntiRaceNamaAgama : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "NamaAgama",
                schema: "public",
                table: "MstAgama",
                type: "citext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Agamas_Nama_Active",
                schema: "public",
                table: "MstAgama",
                column: "NamaAgama",
                unique: true,
                filter: "\"IsDelete\" = false OR \"IsDelete\" IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Agamas_Nama_Active",
                schema: "public",
                table: "MstAgama");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "NamaAgama",
                schema: "public",
                table: "MstAgama",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext");
        }
    }
}
