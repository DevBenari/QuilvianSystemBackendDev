using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtablereseptebus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResepTebus",
                schema: "public",
                columns: table => new
                {
                    ResepTebusId = table.Column<Guid>(type: "uuid", nullable: false),
                    AntrianResep = table.Column<int>(type: "integer", nullable: true),
                    StatusPembuatanResep = table.Column<string>(type: "text", nullable: true),
                    StatusPengambilan = table.Column<bool>(type: "boolean", nullable: true),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: true),
                    IsLunas = table.Column<bool>(type: "boolean", nullable: true),
                    TanggalPembuatanResep = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_ResepTebus", x => x.ResepTebusId);
                });

            migrationBuilder.CreateTable(
                name: "ResepTebusDetail",
                schema: "public",
                columns: table => new
                {
                    ResepTebusDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepTebusId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRacikan = table.Column<bool>(type: "boolean", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    HargaObat = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_ResepTebusDetail", x => x.ResepTebusDetailId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResepTebus",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ResepTebusDetail",
                schema: "public");
        }
    }
}
