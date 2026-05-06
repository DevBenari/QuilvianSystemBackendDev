using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableObatHarga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BentukRacikan",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.AddColumn<Guid>(
                name: "BentukRacikanId",
                schema: "public",
                table: "MstRacikan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsControlled",
                schema: "public",
                table: "MstObat",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KategoriObat",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObatRuteId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstHargaObats",
                schema: "public",
                columns: table => new
                {
                    HargaObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    HargaHNA = table.Column<decimal>(type: "numeric", nullable: true),
                    HargaHTE = table.Column<decimal>(type: "numeric", nullable: true),
                    IsTermasukPajak = table.Column<bool>(type: "boolean", nullable: true),
                    AwalEfektif = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AkhirEfektif = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_MstHargaObats", x => x.HargaObatId);
                });

            migrationBuilder.CreateTable(
                name: "MstRacikanBentuk",
                schema: "public",
                columns: table => new
                {
                    BentukRacikanId = table.Column<Guid>(type: "uuid", nullable: false),
                    LatinBentukRacikan = table.Column<string>(type: "text", nullable: true),
                    NamaBentukRacikan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstRacikanBentuk", x => x.BentukRacikanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstHargaObats",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstRacikanBentuk",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "BentukRacikanId",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.DropColumn(
                name: "IsControlled",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ItemId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "KategoriObat",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ObatRuteId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.AddColumn<string>(
                name: "BentukRacikan",
                schema: "public",
                table: "MstRacikan",
                type: "text",
                nullable: true);
        }
    }
}
