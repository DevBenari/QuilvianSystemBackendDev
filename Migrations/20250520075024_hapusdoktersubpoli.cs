using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hapusdoktersubpoli : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropTable(
                name: "MstDokterSubPoli",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstJadwalPraktek_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstDokterSubPoli",
                schema: "public",
                columns: table => new
                {
                    DokterSubPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubPoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    KodeDokterSubPoli = table.Column<string>(type: "text", nullable: true),
                    NamaDokter = table.Column<string>(type: "text", nullable: false),
                    NamaSubPoli = table.Column<string>(type: "text", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDokterSubPoli", x => x.DokterSubPoliId);
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstAsuransi_AsuransiId",
                        column: x => x.AsuransiId,
                        principalSchema: "public",
                        principalTable: "MstAsuransi",
                        principalColumn: "AsuransiId");
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "public",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MstDokterSubPoli_MstSubPoli_SubPoliId",
                        column: x => x.SubPoliId,
                        principalSchema: "public",
                        principalTable: "MstSubPoli",
                        principalColumn: "SubPoliId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_AsuransiId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_DokterId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDokterSubPoli_SubPoliId",
                schema: "public",
                table: "MstDokterSubPoli",
                column: "SubPoliId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstJadwalPraktek_MstDokterSubPoli_DokterSubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                column: "DokterSubPoliId",
                principalSchema: "public",
                principalTable: "MstDokterSubPoli",
                principalColumn: "DokterSubPoliId");
        }
    }
}
