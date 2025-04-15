using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addmstcmandsp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipePasien",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "MstCurrentMedication",
                schema: "public",
                columns: table => new
                {
                    CurrentMedicationID = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendaftaranPasienBaruId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRekamMedis = table.Column<string>(type: "text", nullable: true),
                    NamaObat = table.Column<string>(type: "text", nullable: true),
                    Dosis = table.Column<string>(type: "text", nullable: true),
                    Frekuensi = table.Column<string>(type: "text", nullable: true),
                    LamaKonsumsi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstCurrentMedication", x => x.CurrentMedicationID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstCurrentMedication",
                schema: "public");

            migrationBuilder.AlterColumn<string>(
                name: "TipePasien",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
