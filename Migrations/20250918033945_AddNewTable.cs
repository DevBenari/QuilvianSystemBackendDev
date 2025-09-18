using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatatanESOs",
                columns: table => new
                {
                    ESOId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    CttPemberianObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTandaiObat = table.Column<bool>(type: "boolean", nullable: true),
                    TglTerjadi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManifestasiESO = table.Column<string>(type: "text", nullable: true),
                    TglKesudahan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerawatUserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDid = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CatatanESOs", x => x.ESOId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatatanESOs");
        }
    }
}
