using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class TabelPaket2Layanan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepositPersentases",
                columns: table => new
                {
                    PersentaseDeposidId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimitPersentase = table.Column<decimal>(type: "numeric", nullable: true),
                    AwalPeriode = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AkhirPeriode = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_DepositPersentases", x => x.PersentaseDeposidId);
                });

            migrationBuilder.CreateTable(
                name: "DiskonPersentases",
                columns: table => new
                {
                    DiskonPercentaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    NominalPersentase = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_DiskonPersentases", x => x.DiskonPercentaseId);
                });

            migrationBuilder.CreateTable(
                name: "MstPaketLayanan",
                schema: "public",
                columns: table => new
                {
                    PaketLayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePaketLayanan = table.Column<string>(type: "text", nullable: true),
                    NamaPaketLayanan = table.Column<string>(type: "text", nullable: true),
                    TglPembuatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LayananId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstPaketLayanan", x => x.PaketLayananId);
                });

            migrationBuilder.CreateTable(
                name: "PaketLayananAsuransis",
                columns: table => new
                {
                    PaketLayananAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaketLayananId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorporateId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPembuatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PaketLayananAsuransis", x => x.PaketLayananAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "PaketLayananDetails",
                columns: table => new
                {
                    DetailPaketLayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailPaketId = table.Column<Guid>(type: "uuid", nullable: true),
                    LayananId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPembuatan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PaketLayananDetails", x => x.DetailPaketLayananId);
                });

            migrationBuilder.CreateTable(
                name: "PaketLayananDiskons",
                columns: table => new
                {
                    DiskonPaketLayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeDiskonPaket = table.Column<string>(type: "text", nullable: true),
                    PaketLayananId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaketLayananAsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiskonPercentageId = table.Column<Guid>(type: "uuid", nullable: true),
                    PotonganHargaMax = table.Column<decimal>(type: "numeric", nullable: true),
                    PeriodeAwal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodeAkhir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PaketLayananDiskons", x => x.DiskonPaketLayananId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositPersentases");

            migrationBuilder.DropTable(
                name: "DiskonPersentases");

            migrationBuilder.DropTable(
                name: "MstPaketLayanan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PaketLayananAsuransis");

            migrationBuilder.DropTable(
                name: "PaketLayananDetails");

            migrationBuilder.DropTable(
                name: "PaketLayananDiskons");
        }
    }
}
