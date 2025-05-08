using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class satuan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsuransiId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.AlterColumn<Guid>(
                name: "BentukObatId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "JumlahSatuan",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SatuanId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstSatuan",
                schema: "public",
                columns: table => new
                {
                    SatuanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeSatuan = table.Column<string>(type: "text", nullable: false),
                    NamaSatuan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstSatuan", x => x.SatuanId);
                });

            migrationBuilder.CreateTable(
                name: "TindakanKunjungans",
                columns: table => new
                {
                    TindakanKunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: true),
                    Disposition = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TindakanKunjungans", x => x.TindakanKunjunganId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstSatuan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TindakanKunjungans");

            migrationBuilder.DropColumn(
                name: "JumlahSatuan",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "SatuanId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.AddColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BentukObatId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
