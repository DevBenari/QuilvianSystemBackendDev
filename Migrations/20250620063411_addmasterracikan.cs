using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addmasterracikan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstRacikan",
                schema: "public",
                columns: table => new
                {
                    RacikanId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaRacikan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstRacikan", x => x.RacikanId);
                });

            migrationBuilder.CreateTable(
                name: "MstRacikanAddon",
                schema: "public",
                columns: table => new
                {
                    AddonRacikanId = table.Column<Guid>(type: "uuid", nullable: false),
                    BentukObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaBentukObat = table.Column<string>(type: "text", nullable: true),
                    BiayaJasaRacikan = table.Column<decimal>(type: "numeric", nullable: true),
                    BiayaKemasanRacikan = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstRacikanAddon", x => x.AddonRacikanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstRacikan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstRacikanAddon",
                schema: "public");
        }
    }
}
