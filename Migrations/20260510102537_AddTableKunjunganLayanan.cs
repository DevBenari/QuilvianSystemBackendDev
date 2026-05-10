using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableKunjunganLayanan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstResepDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObatUnitId",
                schema: "public",
                table: "MstResepDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisLayanan",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KunjunganLayananId",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                schema: "public",
                table: "MstObatUnit",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstDokterInstalasiUnit",
                schema: "public",
                columns: table => new
                {
                    DokterInstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstDokterInstalasiUnit", x => x.DokterInstalasiUnitId);
                    table.ForeignKey(
                        name: "FK_MstDokterInstalasiUnit_Hrd_InstalasiUnit_InstalasiUnitId",
                        column: x => x.InstalasiUnitId,
                        principalSchema: "public",
                        principalTable: "Hrd_InstalasiUnit",
                        principalColumn: "InstalasiUnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstDokterInstalasiUnit_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKunjunganLayanan",
                schema: "public",
                columns: table => new
                {
                    KunjunganLayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstalasiUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    RanapId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    JenisLayanan = table.Column<string>(type: "text", nullable: true),
                    TglMasukLayanan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglKeluarLayanan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstKunjunganLayanan", x => x.KunjunganLayananId);
                    table.ForeignKey(
                        name: "FK_MstKunjunganLayanan_Hrd_InstalasiUnit_InstalasiUnitId",
                        column: x => x.InstalasiUnitId,
                        principalSchema: "public",
                        principalTable: "Hrd_InstalasiUnit",
                        principalColumn: "InstalasiUnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstKunjunganLayanan_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId");
                    table.ForeignKey(
                        name: "FK_MstKunjunganLayanan_MstKunjungan_KunjunganId",
                        column: x => x.KunjunganId,
                        principalSchema: "public",
                        principalTable: "MstKunjungan",
                        principalColumn: "KunjunganID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstKunjunganLayanan_MstPoliklinik_PoliklinikId",
                        column: x => x.PoliklinikId,
                        principalSchema: "public",
                        principalTable: "MstPoliklinik",
                        principalColumn: "PoliklinikId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterInstalasiUnit_DokterId",
                schema: "public",
                table: "MstDokterInstalasiUnit",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterInstalasiUnit_InstalasiUnitId",
                schema: "public",
                table: "MstDokterInstalasiUnit",
                column: "InstalasiUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjunganLayanan_DokterId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjunganLayanan_InstalasiUnitId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "InstalasiUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjunganLayanan_KunjunganId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKunjunganLayanan_PoliklinikId",
                schema: "public",
                table: "MstKunjunganLayanan",
                column: "PoliklinikId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDokterInstalasiUnit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKunjunganLayanan",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "ObatUnitId",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "InstalasiUnitId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "JenisLayanan",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "KunjunganLayananId",
                schema: "public",
                table: "MstResep");

            migrationBuilder.DropColumn(
                name: "Qty",
                schema: "public",
                table: "MstObatUnit");
        }
    }
}
