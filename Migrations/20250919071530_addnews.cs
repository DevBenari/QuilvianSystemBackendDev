using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addnews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Adm_Pembayaran",
                schema: "public",
                table: "Adm_Pembayaran");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Adm_JenisUser",
                schema: "public",
                table: "Adm_JenisUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Adm_JenisPembayaran",
                schema: "public",
                table: "Adm_JenisPembayaran");

            migrationBuilder.RenameTable(
                name: "Adm_Pembayaran",
                schema: "public",
                newName: "JnsPembayaranNominal",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Adm_JenisUser",
                schema: "public",
                newName: "JnsUser",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Adm_JenisPembayaran",
                schema: "public",
                newName: "JnsPembayaran",
                newSchema: "public");

            migrationBuilder.AddColumn<string>(
                name: "Pas",
                schema: "public",
                table: "JnsUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_JnsPembayaranNominal",
                schema: "public",
                table: "JnsPembayaranNominal",
                column: "PembayaranId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JnsUser",
                schema: "public",
                table: "JnsUser",
                column: "JenisUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JnsPembayaran",
                schema: "public",
                table: "JnsPembayaran",
                column: "JenisPembayaranId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JnsUser",
                schema: "public",
                table: "JnsUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JnsPembayaranNominal",
                schema: "public",
                table: "JnsPembayaranNominal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JnsPembayaran",
                schema: "public",
                table: "JnsPembayaran");

            migrationBuilder.DropColumn(
                name: "Pas",
                schema: "public",
                table: "JnsUser");

            migrationBuilder.RenameTable(
                name: "JnsUser",
                schema: "public",
                newName: "Adm_JenisUser",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "JnsPembayaranNominal",
                schema: "public",
                newName: "Adm_Pembayaran",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "JnsPembayaran",
                schema: "public",
                newName: "Adm_JenisPembayaran",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adm_JenisUser",
                schema: "public",
                table: "Adm_JenisUser",
                column: "JenisUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adm_Pembayaran",
                schema: "public",
                table: "Adm_Pembayaran",
                column: "PembayaranId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adm_JenisPembayaran",
                schema: "public",
                table: "Adm_JenisPembayaran",
                column: "JenisPembayaranId");
        }
    }
}
