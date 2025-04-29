using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addnewmst : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "IsSurgery",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "KodeCoveranObat",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "ServiceCodeClass",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "ServiceDesc",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "TglBerakhir",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "TglBerlaku",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.RenameColumn(
                name: "Tarif",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                newName: "TarifObatAsuransi");

            migrationBuilder.AddColumn<decimal>(
                name: "HargaRetail",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KategoriObatId",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NamaKategoriObat",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObatId",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersentaseDiskon",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstDetailICD",
                schema: "public",
                columns: table => new
                {
                    DetailICDId = table.Column<Guid>(type: "uuid", nullable: false),
                    SoapId = table.Column<Guid>(type: "uuid", nullable: true),
                    ICDId = table.Column<Guid>(type: "uuid", nullable: true),
                    isUtama = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstDetailICD", x => x.DetailICDId);
                });

            migrationBuilder.CreateTable(
                name: "MstDetailResep",
                schema: "public",
                columns: table => new
                {
                    DetailResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    InteraturObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstDetailResep", x => x.DetailResepId);
                });

            migrationBuilder.CreateTable(
                name: "MstICD-10",
                schema: "public",
                columns: table => new
                {
                    ICDId = table.Column<Guid>(type: "uuid", nullable: false),
                    ICDCode = table.Column<string>(type: "text", nullable: true),
                    ICDName = table.Column<string>(type: "text", nullable: true),
                    DTDCode = table.Column<string>(type: "text", nullable: true),
                    NamaDiagnosa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstICD-10", x => x.ICDId);
                });

            migrationBuilder.CreateTable(
                name: "MstResep",
                schema: "public",
                columns: table => new
                {
                    ResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstResep", x => x.ResepId);
                });

            migrationBuilder.CreateTable(
                name: "MstSOAP",
                schema: "public",
                columns: table => new
                {
                    SOAPID = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subjective = table.Column<string>(type: "text", nullable: true),
                    Objective = table.Column<string>(type: "text", nullable: true),
                    Assessment = table.Column<string>(type: "text", nullable: true),
                    Planning = table.Column<string>(type: "text", nullable: true),
                    Profesi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSOAP", x => x.SOAPID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDetailICD",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDetailResep",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstICD-10",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstResep",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSOAP",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "HargaRetail",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "KategoriObatId",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "NamaKategoriObat",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "ObatId",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "PersentaseDiskon",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.RenameColumn(
                name: "TarifObatAsuransi",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                newName: "Tarif");

            migrationBuilder.AddColumn<string>(
                name: "Class",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSurgery",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodeCoveranObat",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCodeClass",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceDesc",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TglBerakhir",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TglBerlaku",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);
        }
    }
}
