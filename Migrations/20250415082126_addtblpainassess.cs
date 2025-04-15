using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtblpainassess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "StatusPerkawinan");

            migrationBuilder.AlterColumn<string>(
                name: "Nohp",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nik",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Alamat",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Spesialis",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstPainAssessment",
                schema: "public",
                columns: table => new
                {
                    PainAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    KeluhanUtama = table.Column<string>(type: "text", nullable: true),
                    IsPain = table.Column<bool>(type: "boolean", nullable: true),
                    Pemicu = table.Column<string>(type: "text", nullable: true),
                    Kualitas = table.Column<string>(type: "text", nullable: true),
                    Lokasi = table.Column<string>(type: "text", nullable: true),
                    SkalaPainId = table.Column<Guid>(type: "uuid", nullable: true),
                    Frekuensi = table.Column<string>(type: "text", nullable: true),
                    PainManagement = table.Column<string>(type: "text", nullable: true),
                    IsInheritedDisease = table.Column<bool>(type: "boolean", nullable: true),
                    InheritedDisease = table.Column<string>(type: "text", nullable: true),
                    IsAlergic = table.Column<bool>(type: "boolean", nullable: true),
                    Alergic = table.Column<string>(type: "text", nullable: true),
                    NafsuMakan = table.Column<string>(type: "text", nullable: true),
                    IsMual = table.Column<bool>(type: "boolean", nullable: true),
                    IsMuntah = table.Column<bool>(type: "boolean", nullable: true),
                    IsFallRisk = table.Column<bool>(type: "boolean", nullable: true),
                    FallRisk = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstPainAssessment", x => x.PainAssessmentId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPainAssessment",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Spesialis",
                schema: "public",
                table: "MstDokter");

            migrationBuilder.RenameColumn(
                name: "StatusPerkawinan",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Nohp",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nik",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alamat",
                schema: "public",
                table: "MstDokter",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
