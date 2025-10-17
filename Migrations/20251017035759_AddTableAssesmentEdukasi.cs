using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableAssesmentEdukasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssesmentEdukasis",
                columns: table => new
                {
                    AsesmenEdukasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    BahasaDigunakan = table.Column<string>(type: "text", nullable: true),
                    IsKebutuhanPenerjemah = table.Column<bool>(type: "boolean", nullable: true),
                    IsBacaTulis = table.Column<bool>(type: "boolean", nullable: true),
                    TipePembelajaran = table.Column<string>(type: "text", nullable: true),
                    NilaiKepercayaan = table.Column<string>(type: "text", nullable: true),
                    PendidikanId = table.Column<Guid>(type: "uuid", nullable: true),
                    HambatanEdukasi = table.Column<string>(type: "text", nullable: true),
                    IsMenerimaEdukasi = table.Column<bool>(type: "boolean", nullable: true),
                    KebutuhanEdukasi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AssesmentEdukasis", x => x.AsesmenEdukasiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssesmentEdukasis");
        }
    }
}
