using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class mapingdanGL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "test",
                schema: "public",
                table: "Fin_GrupCoa");

            migrationBuilder.CreateTable(
                name: "Fin_COAMapping",
                schema: "public",
                columns: table => new
                {
                    COAMappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransaksiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaTransaksi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    COAId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaCOA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_COAMapping", x => x.COAMappingId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_GLDetail",
                schema: "public",
                columns: table => new
                {
                    GLDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    GLHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    COAId = table.Column<Guid>(type: "uuid", nullable: false),
                    NilaiDebit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NilaiKredit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SourceItemType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceItem = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_GLDetail", x => x.GLDetailId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_GLHeader",
                schema: "public",
                columns: table => new
                {
                    GLHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    GLKode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoRegistrasi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    JenisKunjungan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglTransaksi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglPosting = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceGL = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceTypeGL = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GLStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_GLHeader", x => x.GLHeaderId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_COAMapping",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_GLDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_GLHeader",
                schema: "public");

            migrationBuilder.AddColumn<string>(
                name: "test",
                schema: "public",
                table: "Fin_GrupCoa",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
