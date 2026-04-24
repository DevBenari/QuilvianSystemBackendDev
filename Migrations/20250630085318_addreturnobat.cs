using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addreturnobat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QtyItem",
                schema: "public",
                table: "Billing",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ObatReturn",
                schema: "public",
                columns: table => new
                {
                    ObatReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    KasirId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusPembayaran = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TanggalReturn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ObatReturn", x => x.ObatReturnId);
                });

            migrationBuilder.CreateTable(
                name: "ObatReturnDetail",
                schema: "public",
                columns: table => new
                {
                    ObatReturnDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatReturnId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaObat = table.Column<string>(type: "text", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    NoBatch = table.Column<string>(type: "text", nullable: true),
                    IsMasihTersegel = table.Column<bool>(type: "boolean", nullable: true),
                    IsObatUtuh = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_ObatReturnDetail", x => x.ObatReturnDetailId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObatReturn",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ObatReturnDetail",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "QtyItem",
                schema: "public",
                table: "Billing");
        }
    }
}
