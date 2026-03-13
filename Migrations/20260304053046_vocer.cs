using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class vocer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VoucherPettyCash",
                schema: "public",
                columns: table => new
                {
                    VoucherPettyCashId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeVoucherPC = table.Column<string>(type: "text", nullable: true),
                    LayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KasirId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftSesi = table.Column<string>(type: "text", nullable: true),
                    NamaPenerima = table.Column<string>(type: "text", nullable: true),
                    TanggalPengajuan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KategoriVoucher = table.Column<string>(type: "text", nullable: true),
                    NominalVoucher = table.Column<decimal>(type: "numeric", nullable: true),
                    BuktiNota = table.Column<string>(type: "text", nullable: true),
                    StatusVoucher = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherPettyCash", x => x.VoucherPettyCashId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoucherPettyCash",
                schema: "public");
        }
    }
}
