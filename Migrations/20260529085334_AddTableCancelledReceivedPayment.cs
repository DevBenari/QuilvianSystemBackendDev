using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableCancelledReceivedPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstLab_LabId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_LabId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropColumn(
                name: "LabId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.AddColumn<Guid>(
                name: "AyatSilangId",
                schema: "public",
                table: "Fin_ReceivedPayment",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Fin_CanceledReceivedPayment",
                schema: "public",
                columns: table => new
                {
                    CancelledReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRef = table.Column<string>(type: "text", nullable: true),
                    CancelledOperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Fin_CanceledReceivedPayment", x => x.CancelledReceivedPaymentId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_CanceledReceivedPayment",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "AyatSilangId",
                schema: "public",
                table: "Fin_ReceivedPayment");

            migrationBuilder.AddColumn<Guid>(
                name: "LabId",
                schema: "public",
                table: "LabBooking",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_LabId",
                schema: "public",
                table: "LabBooking",
                column: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstLab_LabId",
                schema: "public",
                table: "LabBooking",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");
        }
    }
}
