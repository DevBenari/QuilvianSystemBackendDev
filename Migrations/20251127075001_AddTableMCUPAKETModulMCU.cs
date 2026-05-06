using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableMCUPAKETModulMCU : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartementId",
                table: "RuangBedahBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstModulMCU",
                schema: "public",
                columns: table => new
                {
                    ModulMCUId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaModul = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstModulMCU", x => x.ModulMCUId);
                });

            migrationBuilder.CreateTable(
                name: "PaketMCUs",
                columns: table => new
                {
                    PaketMCUId = table.Column<Guid>(type: "uuid", nullable: false),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModulMCUId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterID = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PaketMCUs", x => x.PaketMCUId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstModulMCU",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PaketMCUs");

            migrationBuilder.DropColumn(
                name: "DepartementId",
                table: "RuangBedahBookings");
        }
    }
}
