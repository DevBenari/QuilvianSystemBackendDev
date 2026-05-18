using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newTambahan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BiayaAdminBank",
                schema: "public",
                table: "Hrd_MasterBank",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PPH23Nom",
                schema: "public",
                table: "Fin_DetailReceivedPayment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PPH23Per",
                schema: "public",
                table: "Fin_DetailReceivedPayment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedVp",
                schema: "public",
                table: "Fin_DetailInvoiceReceived",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileBuktiPembayaran",
                schema: "public",
                table: "Fin_DetailInvoiceReceived",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PembayaranKe",
                schema: "public",
                table: "Fin_DetailInvoiceReceived",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PiutangTerbayar",
                schema: "public",
                table: "Fin_DetailInvoiceReceived",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FIN_ARCanceled",
                schema: "public",
                columns: table => new
                {
                    ARCanceledId = table.Column<Guid>(type: "uuid", nullable: false),
                    ARHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CanceledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    CanceledOperatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaCanceledOperator = table.Column<string>(type: "text", nullable: false),
                    CanceledReason = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ARCanceled", x => x.ARCanceledId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIN_ARCanceled",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "BiayaAdminBank",
                schema: "public",
                table: "Hrd_MasterBank");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "PPH23Nom",
                schema: "public",
                table: "Fin_DetailReceivedPayment");

            migrationBuilder.DropColumn(
                name: "PPH23Per",
                schema: "public",
                table: "Fin_DetailReceivedPayment");

            migrationBuilder.DropColumn(
                name: "ApprovedVp",
                schema: "public",
                table: "Fin_DetailInvoiceReceived");

            migrationBuilder.DropColumn(
                name: "FileBuktiPembayaran",
                schema: "public",
                table: "Fin_DetailInvoiceReceived");

            migrationBuilder.DropColumn(
                name: "PembayaranKe",
                schema: "public",
                table: "Fin_DetailInvoiceReceived");

            migrationBuilder.DropColumn(
                name: "PiutangTerbayar",
                schema: "public",
                table: "Fin_DetailInvoiceReceived");
        }
    }
}
