using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystem.Migrations
{
    public partial class add_wilayah_dokter_praktek : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterUserActive",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "Dokters",
                columns: table => new
                {
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KdDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NmDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Str = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TglSip = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TglStr = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PanggilDokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nik = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_Dokters", x => x.DokterId);
                });

            migrationBuilder.CreateTable(
                name: "MasterPegawai",
                schema: "dbo",
                columns: table => new
                {
                    UserActiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserActiveCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handphone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_MasterPegawai", x => x.UserActiveId);
                });

            migrationBuilder.CreateTable(
                name: "MstProvinsi",
                schema: "dbo",
                columns: table => new
                {
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinsiCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinsiName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstProvinsi", x => x.ProvinsiId);
                });

            migrationBuilder.CreateTable(
                name: "DokterPrakteks",
                columns: table => new
                {
                    DokterPraktekId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Layanan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamPraktek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hari = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamMasuk = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamKeluar = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DokterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_DokterPrakteks", x => x.DokterPraktekId);
                    table.ForeignKey(
                        name: "FK_DokterPrakteks_Dokters_DokterId",
                        column: x => x.DokterId,
                        principalTable: "Dokters",
                        principalColumn: "DokterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKabupaten",
                schema: "dbo",
                columns: table => new
                {
                    KabupatenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KabupatenCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKabupaten", x => x.KabupatenId);
                    table.ForeignKey(
                        name: "FK_MstKabupaten_MstProvinsi_ProvinsiId",
                        column: x => x.ProvinsiId,
                        principalSchema: "dbo",
                        principalTable: "MstProvinsi",
                        principalColumn: "ProvinsiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKecamatan",
                schema: "dbo",
                columns: table => new
                {
                    KecamatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KecamatanCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KecamatanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKecamatan", x => x.KecamatanId);
                    table.ForeignKey(
                        name: "FK_MstKecamatan_MstKabupaten_KabupatenId",
                        column: x => x.KabupatenId,
                        principalSchema: "dbo",
                        principalTable: "MstKabupaten",
                        principalColumn: "KabupatenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstKelurahan",
                schema: "dbo",
                columns: table => new
                {
                    KelurahanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KelurahanCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KelurahanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KecamatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKelurahan", x => x.KelurahanId);
                    table.ForeignKey(
                        name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                        column: x => x.KecamatanId,
                        principalSchema: "dbo",
                        principalTable: "MstKecamatan",
                        principalColumn: "KecamatanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DokterPrakteks_DokterId",
                table: "DokterPrakteks",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKabupaten_ProvinsiId",
                schema: "dbo",
                table: "MstKabupaten",
                column: "ProvinsiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKecamatan_KabupatenId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKelurahan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan",
                column: "KecamatanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DokterPrakteks");

            migrationBuilder.DropTable(
                name: "MasterPegawai",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKelurahan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Dokters");

            migrationBuilder.DropTable(
                name: "MstKecamatan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKabupaten",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstProvinsi",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "MasterUserActive",
                schema: "dbo",
                columns: table => new
                {
                    UserActiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handphone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserActiveCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterUserActive", x => x.UserActiveId);
                });
        }
    }
}
