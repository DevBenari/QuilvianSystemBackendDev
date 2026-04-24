using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtblbilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiayaAdministrasiKode",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "MetodePembayaranId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.DropColumn(
                name: "StatusPembayaran",
                schema: "public",
                table: "MainKasir");

            migrationBuilder.RenameColumn(
                name: "NominalPembayaran",
                schema: "public",
                table: "MainKasir",
                newName: "GrandTotalPembayaran");

            migrationBuilder.CreateTable(
                name: "Billing",
                schema: "public",
                columns: table => new
                {
                    BillingId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingKode = table.Column<string>(type: "text", nullable: true),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaItem = table.Column<string>(type: "text", nullable: true),
                    HargaItem = table.Column<decimal>(type: "numeric", nullable: true),
                    SubTotalItem = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_Billing", x => x.BillingId);
                });

            migrationBuilder.CreateTable(
                name: "MainKasirDetail",
                schema: "public",
                columns: table => new
                {
                    MainKasirDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    MainKasirId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetodePembayaranId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaMetode = table.Column<string>(type: "text", nullable: true),
                    NominalPembayaran = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    StatusPembayaran = table.Column<bool>(type: "boolean", nullable: true),
                    TglPembayaran = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_MainKasirDetail", x => x.MainKasirDetailId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Billing",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MainKasirDetail",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "GrandTotalPembayaran",
                schema: "public",
                table: "MainKasir",
                newName: "NominalPembayaran");

            migrationBuilder.AddColumn<string>(
                name: "BiayaAdministrasiKode",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MetodePembayaranId",
                schema: "public",
                table: "MainKasir",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                schema: "public",
                table: "MainKasir",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusPembayaran",
                schema: "public",
                table: "MainKasir",
                type: "text",
                nullable: true);
        }
    }
}
