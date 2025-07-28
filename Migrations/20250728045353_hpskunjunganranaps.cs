using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hpskunjunganranaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KunjunganRanap",
                schema: "public");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KunjunganRanap",
                schema: "public",
                columns: table => new
                {
                    KunjunganRanapId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    BedId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DokterDPJPId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCito = table.Column<bool>(type: "boolean", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrioritas = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    KeteranganSelesaiRanap = table.Column<string>(type: "text", nullable: true),
                    KodeKunjungan = table.Column<string>(type: "text", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusRanap = table.Column<bool>(type: "boolean", nullable: true),
                    SuratPengantarId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglAdministrasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipePembayaran = table.Column<string>(type: "text", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KunjunganRanap", x => x.KunjunganRanapId);
                });
        }
    }
}
