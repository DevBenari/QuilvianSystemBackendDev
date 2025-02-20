using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addTablePoliklinik : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    LayananPoliklinik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JumlahMaxPasien = table.Column<int>(type: "int", nullable: false),
                    Deskripsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPoliklinik",
                schema: "dbo");
        }
    }
}
