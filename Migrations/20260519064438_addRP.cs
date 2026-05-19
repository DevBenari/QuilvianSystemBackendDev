using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addRP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DetailReceivedPaymentId",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "NamaAsuransi",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPasien",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPerusahaan",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoInvoice",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoRm",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSaldoAwal",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailReceivedPaymentId",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "NamaAsuransi",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "NamaPasien",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "NamaPerusahaan",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "NoInvoice",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "NoRm",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.DropColumn(
                name: "TotalSaldoAwal",
                schema: "public",
                table: "Fin_ReceivedPayment");
        }
    }
}
