using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class spri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KodeRacikan",
                schema: "public",
                table: "MstRacikan",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SuratPengantarRawatInaps",
                columns: table => new
                {
                    SuratPengantarRawatInapId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    NomorSuratPengantar = table.Column<string>(type: "text", nullable: true),
                    Diagnosa = table.Column<string>(type: "text", nullable: true),
                    ICDId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlasanRanap = table.Column<string>(type: "text", nullable: true),
                    RencanaTindakLanjut = table.Column<string>(type: "text", nullable: true),
                    AsalUnit = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SuratPengantarRawatInaps", x => x.SuratPengantarRawatInapId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuratPengantarRawatInaps");

            migrationBuilder.DropColumn(
                name: "KodeRacikan",
                schema: "public",
                table: "MstRacikan");
        }
    }
}
