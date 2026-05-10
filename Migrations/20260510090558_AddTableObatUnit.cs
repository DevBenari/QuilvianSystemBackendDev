using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableObatUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HargaJual",
                schema: "public",
                table: "MstObat",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FIN_ARDetail",
                schema: "public",
                columns: table => new
                {
                    ARDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ARHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoRM = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    NoBilling = table.Column<string>(type: "text", nullable: false),
                    NoRegistrasi = table.Column<string>(type: "text", nullable: false),
                    TglKunjungan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalPiutang = table.Column<decimal>(type: "numeric", nullable: false),
                    DiskonTagihan = table.Column<decimal>(type: "numeric", nullable: false),
                    SelisihTagihan = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSetelahDiskon = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ARDetail", x => x.ARDetailId);
                });

            migrationBuilder.CreateTable(
                name: "FIN_ARDokumen",
                schema: "public",
                columns: table => new
                {
                    ARDokumenId = table.Column<Guid>(type: "uuid", nullable: false),
                    ARHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoRM = table.Column<string>(type: "text", nullable: false),
                    NamaPasien = table.Column<string>(type: "text", nullable: false),
                    DokTagihanPerawatan = table.Column<string>(type: "text", nullable: false),
                    DokDetailBiaya = table.Column<string>(type: "text", nullable: false),
                    TglTerimaDok = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ARDokumen", x => x.ARDokumenId);
                });

            migrationBuilder.CreateTable(
                name: "FIN_ARHeader",
                schema: "public",
                columns: table => new
                {
                    ARHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    TglPembuatanInvoice = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<int>(type: "integer", nullable: false),
                    TotalInvoice = table.Column<decimal>(type: "numeric", nullable: false),
                    TglKirim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglTerima = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglTagihan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglJatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDocumentComplited = table.Column<bool>(type: "boolean", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ARHeader", x => x.ARHeaderId);
                });

            migrationBuilder.CreateTable(
                name: "MstObatUnit",
                schema: "public",
                columns: table => new
                {
                    ObatUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    QtyAmbil = table.Column<decimal>(type: "numeric", nullable: true),
                    QtyTersedia = table.Column<decimal>(type: "numeric", nullable: true),
                    InstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstObatUnit", x => x.ObatUnitId);
                    table.ForeignKey(
                        name: "FK_MstObatUnit_Hrd_InstalasiUnit_InstalasiUnitId",
                        column: x => x.InstalasiUnitId,
                        principalSchema: "public",
                        principalTable: "Hrd_InstalasiUnit",
                        principalColumn: "InstalasiUnitId");
                    table.ForeignKey(
                        name: "FK_MstObatUnit_MstObat_ObatId",
                        column: x => x.ObatId,
                        principalSchema: "public",
                        principalTable: "MstObat",
                        principalColumn: "ObatId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstObatUnit_InstalasiUnitId",
                schema: "public",
                table: "MstObatUnit",
                column: "InstalasiUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstObatUnit_ObatId",
                schema: "public",
                table: "MstObatUnit",
                column: "ObatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIN_ARDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FIN_ARDokumen",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FIN_ARHeader",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatUnit",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "HargaJual",
                schema: "public",
                table: "MstObat");
        }
    }
}
