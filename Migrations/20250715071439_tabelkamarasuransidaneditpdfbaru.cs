using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class tabelkamarasuransidaneditpdfbaru : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NamaKontakDarurat",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NamaWali3");

            migrationBuilder.AddColumn<string>(
                name: "NamaWali2",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KamarAsuransi",
                schema: "public",
                columns: table => new
                {
                    KamarAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KamarAsuransi", x => x.KamarAsuransiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KamarAsuransi",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "NamaWali2",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.RenameColumn(
                name: "NamaWali3",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NamaKontakDarurat");
        }
    }
}
