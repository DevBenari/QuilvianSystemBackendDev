using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class inisiasiWilayah : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstNegara",
                schema: "dbo",
                columns: table => new
                {
                    NegaraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeNegara = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaNegara = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_MstNegara", x => x.NegaraId);
                });

            migrationBuilder.CreateTable(
                name: "MstProvinsi",
                schema: "dbo",
                columns: table => new
                {
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeProvinsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaProvinsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NegaraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MstProvinsi", x => x.ProvinsiId);
                    table.ForeignKey(
                        name: "FK_MstProvinsi_MstNegara_NegaraId",
                        column: x => x.NegaraId,
                        principalSchema: "dbo",
                        principalTable: "MstNegara",
                        principalColumn: "NegaraId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstKabupatenKota",
                schema: "dbo",
                columns: table => new
                {
                    KabupatenKotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeKabupatenKota = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKabupatenKota = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MstKabupatenKota", x => x.KabupatenKotaId);
                    table.ForeignKey(
                        name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                        column: x => x.ProvinsiId,
                        principalSchema: "dbo",
                        principalTable: "MstProvinsi",
                        principalColumn: "ProvinsiId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstKecamatan",
                schema: "dbo",
                columns: table => new
                {
                    KecamatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeKecamatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKecamatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenKotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MstKecamatan", x => x.KecamatanId);
                    table.ForeignKey(
                        name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                        column: x => x.KabupatenKotaId,
                        principalSchema: "dbo",
                        principalTable: "MstKabupatenKota",
                        principalColumn: "KabupatenKotaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstKelurahan",
                schema: "dbo",
                columns: table => new
                {
                    KelurahanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeKelurahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKelurahan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KecamatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MstKelurahan", x => x.KelurahanId);
                    table.ForeignKey(
                        name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                        column: x => x.KecamatanId,
                        principalSchema: "dbo",
                        principalTable: "MstKecamatan",
                        principalColumn: "KecamatanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstKabupatenKota_ProvinsiId",
                schema: "dbo",
                table: "MstKabupatenKota",
                column: "ProvinsiId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKecamatan_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenKotaId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKelurahan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan",
                column: "KecamatanId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProvinsi_NegaraId",
                schema: "dbo",
                table: "MstProvinsi",
                column: "NegaraId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstKelurahan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKecamatan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKabupatenKota",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstProvinsi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstNegara",
                schema: "dbo");
        }
    }
}
