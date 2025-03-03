using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDokter",
                schema: "dbo",
                columns: table => new
                {
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KdDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NmDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Str = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TglSip = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TglStr = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nohp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FotoDokter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDokter", x => x.DokterId);
                });

            migrationBuilder.CreateTable(
                name: "MstPoliklinik",
                schema: "dbo",
                columns: table => new
                {
                    PoliklinikId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodePoliklinik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPoliklinik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KepalaPoliklinik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lokasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telepon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HariOperasional = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamBuka = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamTutup = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LayananPoliklinik = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deskripsi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPoliklinik", x => x.PoliklinikId);
                });

            migrationBuilder.CreateTable(
                name: "MstSubPoli",
                schema: "dbo",
                columns: table => new
                {
                    SubPoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaSubPoli = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deskripsi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KepalaSubPoli = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lokasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telepon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HariOperasional = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamBuka = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamTutup = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LayananSubPoli = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstSubPoli", x => x.SubPoliId);
                    table.ForeignKey(
                        name: "FK_MstSubPoli_MstPoliklinik_PoliId",
                        column: x => x.PoliId,
                        principalSchema: "dbo",
                        principalTable: "MstPoliklinik",
                        principalColumn: "PoliklinikId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DokterPolis",
                columns: table => new
                {
                    DokterPoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NamaDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubPoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JadwalPraktekId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DokterPolis", x => x.DokterPoliId);
                    table.ForeignKey(
                        name: "FK_DokterPolis_MstDokter_DokterId",
                        column: x => x.DokterId,
                        principalSchema: "dbo",
                        principalTable: "MstDokter",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DokterPolis_MstPoliklinik_PoliId",
                        column: x => x.PoliId,
                        principalSchema: "dbo",
                        principalTable: "MstPoliklinik",
                        principalColumn: "PoliklinikId");
                    table.ForeignKey(
                        name: "FK_DokterPolis_MstSubPoli_SubPoliId",
                        column: x => x.SubPoliId,
                        principalSchema: "dbo",
                        principalTable: "MstSubPoli",
                        principalColumn: "SubPoliId");
                });

            migrationBuilder.CreateTable(
                name: "MstJadwalPraktek",
                schema: "dbo",
                columns: table => new
                {
                    JadwalPraktekId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamaDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubPoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KodeJadwalPraktek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaktuPraktek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HariPraktek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamMulai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamBerakhir = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxPasien = table.Column<int>(type: "int", nullable: false),
                    DokterPoliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstJadwalPraktek", x => x.JadwalPraktekId);
                    table.ForeignKey(
                        name: "FK_MstJadwalPraktek_DokterPolis_DokterPoliId",
                        column: x => x.DokterPoliId,
                        principalTable: "DokterPolis",
                        principalColumn: "DokterPoliId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_DokterId",
                table: "DokterPolis",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_PoliId",
                table: "DokterPolis",
                column: "PoliId");

            migrationBuilder.CreateIndex(
                name: "IX_DokterPolis_SubPoliId",
                table: "DokterPolis",
                column: "SubPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstJadwalPraktek_DokterPoliId",
                schema: "dbo",
                table: "MstJadwalPraktek",
                column: "DokterPoliId");

            migrationBuilder.CreateIndex(
                name: "IX_MstSubPoli_PoliId",
                schema: "dbo",
                table: "MstSubPoli",
                column: "PoliId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstJadwalPraktek",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DokterPolis");

            migrationBuilder.DropTable(
                name: "MstDokter",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstSubPoli",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstPoliklinik",
                schema: "dbo");
        }
    }
}
