using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePenerimaanDarahPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GolonganDarahId",
                table: "PenerimaanDarahs");

            migrationBuilder.DropColumn(
                name: "PasienId",
                table: "PenerimaanDarahs");

            migrationBuilder.RenameColumn(
                name: "GolonganDarahId",
                table: "StockDarahs",
                newName: "DarahDetailId");

            migrationBuilder.RenameColumn(
                name: "TglMasuk",
                table: "PenerimaanDarahs",
                newName: "TglPenerimaan");

            migrationBuilder.RenameColumn(
                name: "TglExpired",
                table: "PenerimaanDarahs",
                newName: "TglFaktur");

            migrationBuilder.RenameColumn(
                name: "Sumber",
                table: "PenerimaanDarahs",
                newName: "NoPO");

            migrationBuilder.RenameColumn(
                name: "Rhesus",
                table: "PenerimaanDarahs",
                newName: "NoFaktur");

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "StockDarahs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                table: "PenerimaanDarahs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                table: "PenerimaanDarahs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DarahDetailId",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodePenerimaan",
                table: "PenerimaanDarahs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PenerimaId",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KhususUnit",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DarahDetails",
                columns: table => new
                {
                    DarahDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: true),
                    DarahId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rhesus = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DarahDetails", x => x.DarahDetailId);
                });

            migrationBuilder.CreateTable(
                name: "PenerimaDarahPasiens",
                columns: table => new
                {
                    PenerimaanDarahPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rhesus = table.Column<string>(type: "text", nullable: true),
                    JumlahKantong = table.Column<decimal>(type: "numeric", nullable: true),
                    Sumber = table.Column<string>(type: "text", nullable: true),
                    TglMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglExpired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PenerimaDarahPasiens", x => x.PenerimaanDarahPasienId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DarahDetails");

            migrationBuilder.DropTable(
                name: "PenerimaDarahPasiens");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "StockDarahs");

            migrationBuilder.DropColumn(
                name: "DarahDetailId",
                table: "PenerimaanDarahs");

            migrationBuilder.DropColumn(
                name: "KodePenerimaan",
                table: "PenerimaanDarahs");

            migrationBuilder.DropColumn(
                name: "PenerimaId",
                table: "PenerimaanDarahs");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "PenerimaanDarahs");

            migrationBuilder.DropColumn(
                name: "KhususUnit",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.RenameColumn(
                name: "DarahDetailId",
                table: "StockDarahs",
                newName: "GolonganDarahId");

            migrationBuilder.RenameColumn(
                name: "TglPenerimaan",
                table: "PenerimaanDarahs",
                newName: "TglMasuk");

            migrationBuilder.RenameColumn(
                name: "TglFaktur",
                table: "PenerimaanDarahs",
                newName: "TglExpired");

            migrationBuilder.RenameColumn(
                name: "NoPO",
                table: "PenerimaanDarahs",
                newName: "Sumber");

            migrationBuilder.RenameColumn(
                name: "NoFaktur",
                table: "PenerimaanDarahs",
                newName: "Rhesus");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDateTime",
                table: "PenerimaanDarahs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeleteDateTime",
                table: "PenerimaanDarahs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "GolonganDarahId",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PasienId",
                table: "PenerimaanDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
