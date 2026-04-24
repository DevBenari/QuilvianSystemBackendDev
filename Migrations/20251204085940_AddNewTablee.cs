using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewTablee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResepTelaahs",
                columns: table => new
                {
                    TelaahResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAdministratif = table.Column<bool>(type: "boolean", nullable: false),
                    IsNamaObatdanKetersediaan = table.Column<bool>(type: "boolean", nullable: false),
                    IsDosisdanJumlahObat = table.Column<bool>(type: "boolean", nullable: false),
                    IsAturandanCaraPenggunaan = table.Column<bool>(type: "boolean", nullable: false),
                    IsTepatDosis = table.Column<bool>(type: "boolean", nullable: false),
                    IsTepatWaktu = table.Column<bool>(type: "boolean", nullable: false),
                    IsDuplikasi = table.Column<bool>(type: "boolean", nullable: false),
                    IsPolifarmasi = table.Column<bool>(type: "boolean", nullable: false),
                    IsAlergi = table.Column<bool>(type: "boolean", nullable: false),
                    IsKontradiksi = table.Column<bool>(type: "boolean", nullable: false),
                    IsInteraksiObat = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ResepTelaahs", x => x.TelaahResepId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResepTelaahs");
        }
    }
}
