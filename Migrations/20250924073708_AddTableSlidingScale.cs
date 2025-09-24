using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableSlidingScale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlidingScales",
                columns: table => new
                {
                    SlidingScaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglSlidingScale = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GDS = table.Column<decimal>(type: "numeric", nullable: true),
                    Insulin = table.Column<string>(type: "text", nullable: true),
                    InsulinDrip = table.Column<string>(type: "text", nullable: true),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_SlidingScales", x => x.SlidingScaleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlidingScales");
        }
    }
}
