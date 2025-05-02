using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addmstbaruu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPKS",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.AddColumn<string>(
                name: "Kelas",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaObat",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoveranTindakanAsuransi",
                schema: "public",
                columns: table => new
                {
                    CoveranTindakanAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokterAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRsAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJpAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahpAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLainAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotalAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
                    KSOAsuransi = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_CoveranTindakanAsuransi", x => x.CoveranTindakanAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstKelas",
                schema: "public",
                columns: table => new
                {
                    KelasId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeKelas = table.Column<string>(type: "text", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    DeskripsiKelas = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstKelas", x => x.KelasId);
                });

            migrationBuilder.CreateTable(
                name: "MstTarifTindakan",
                schema: "public",
                columns: table => new
                {
                    TarifTindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstTarifTindakan", x => x.TarifTindakanId);
                });

            migrationBuilder.CreateTable(
                name: "MstTindakan",
                schema: "public",
                columns: table => new
                {
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeTindakan = table.Column<string>(type: "text", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    KategoriTindakan = table.Column<string>(type: "text", nullable: true),
                    DeskripsiTindakan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstTindakan", x => x.TindakanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoveranTindakanAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKelas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTarifTindakan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTindakan",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Kelas",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.DropColumn(
                name: "NamaObat",
                schema: "public",
                table: "MstCoveranObatAsuransi");

            migrationBuilder.AddColumn<bool>(
                name: "IsPKS",
                schema: "public",
                table: "MstCoveranObatAsuransi",
                type: "boolean",
                nullable: true);
        }
    }
}
