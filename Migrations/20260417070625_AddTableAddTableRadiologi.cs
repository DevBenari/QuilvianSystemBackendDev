using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableAddTableRadiologi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TarifRadiologis",
                columns: table => new
                {
                    TarifRadId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TarifRadiologis", x => x.TarifRadId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TarifRadiologis");
        }
    }
}
