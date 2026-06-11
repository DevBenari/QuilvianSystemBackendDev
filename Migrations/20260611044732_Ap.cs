using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class Ap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItem_PurchaseOrder_PurchaseOrderId",
                schema: "public",
                table: "PurchaseOrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_ARHeader",
                schema: "public",
                table: "FIN_ARHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_ARDokumen",
                schema: "public",
                table: "FIN_ARDokumen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_ARDetail",
                schema: "public",
                table: "FIN_ARDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_ARCanceled",
                schema: "public",
                table: "FIN_ARCanceled");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrderItem",
                schema: "public",
                table: "PurchaseOrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrder",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.RenameTable(
                name: "FIN_ARHeader",
                schema: "public",
                newName: "Fin_ARHeader",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FIN_ARDokumen",
                schema: "public",
                newName: "Fin_ARDokumen",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FIN_ARDetail",
                schema: "public",
                newName: "Fin_ARDetail",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FIN_ARCanceled",
                schema: "public",
                newName: "Fin_ARCanceled",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PurchaseOrderItem",
                schema: "public",
                newName: "Fin_PurchaseOrderItem",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PurchaseOrder",
                schema: "public",
                newName: "Fin_PurchaseOrder",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderItem_PurchaseOrderId",
                schema: "public",
                table: "Fin_PurchaseOrderItem",
                newName: "IX_Fin_PurchaseOrderItem_PurchaseOrderId");

            migrationBuilder.AddColumn<decimal>(
                name: "SisaPembayaran",
                schema: "public",
                table: "Fin_DetailInvoiceReceived",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_ARHeader",
                schema: "public",
                table: "Fin_ARHeader",
                column: "ARHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_ARDokumen",
                schema: "public",
                table: "Fin_ARDokumen",
                column: "ARDokumenId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_ARDetail",
                schema: "public",
                table: "Fin_ARDetail",
                column: "ARDetailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_ARCanceled",
                schema: "public",
                table: "Fin_ARCanceled",
                column: "ARCanceledId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_PurchaseOrderItem",
                schema: "public",
                table: "Fin_PurchaseOrderItem",
                column: "PurchaseOrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_PurchaseOrder",
                schema: "public",
                table: "Fin_PurchaseOrder",
                column: "PurchaseOrderId");

            migrationBuilder.CreateTable(
                name: "Fin_ReceiveOrder",
                schema: "public",
                columns: table => new
                {
                    ReceiveOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiveOrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsInvoiceProvided = table.Column<bool>(type: "boolean", nullable: true),
                    DeliveryNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TermOfPayment = table.Column<decimal>(type: "numeric", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    StampDuty = table.Column<decimal>(type: "numeric", nullable: true),
                    AdditionalDiscountRp = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_ReceiveOrder", x => x.ReceiveOrderId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_ReceiveOrderItem",
                schema: "public",
                columns: table => new
                {
                    ReceiveOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiveOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Measure = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QtyOrder = table.Column<decimal>(type: "numeric", nullable: true),
                    QtyReceive = table.Column<decimal>(type: "numeric", nullable: true),
                    StampDuty = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_ReceiveOrderItem", x => x.ReceiveOrderItemId);
                    table.ForeignKey(
                        name: "FK_Fin_ReceiveOrderItem_Fin_ReceiveOrder_ReceiveOrderId",
                        column: x => x.ReceiveOrderId,
                        principalSchema: "public",
                        principalTable: "Fin_ReceiveOrder",
                        principalColumn: "ReceiveOrderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fin_ReceiveOrderItem_ReceiveOrderId",
                schema: "public",
                table: "Fin_ReceiveOrderItem",
                column: "ReceiveOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fin_PurchaseOrderItem_Fin_PurchaseOrder_PurchaseOrderId",
                schema: "public",
                table: "Fin_PurchaseOrderItem",
                column: "PurchaseOrderId",
                principalSchema: "public",
                principalTable: "Fin_PurchaseOrder",
                principalColumn: "PurchaseOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fin_PurchaseOrderItem_Fin_PurchaseOrder_PurchaseOrderId",
                schema: "public",
                table: "Fin_PurchaseOrderItem");

            migrationBuilder.DropTable(
                name: "Fin_ReceiveOrderItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_ReceiveOrder",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_ARHeader",
                schema: "public",
                table: "Fin_ARHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_ARDokumen",
                schema: "public",
                table: "Fin_ARDokumen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_ARDetail",
                schema: "public",
                table: "Fin_ARDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_ARCanceled",
                schema: "public",
                table: "Fin_ARCanceled");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_PurchaseOrderItem",
                schema: "public",
                table: "Fin_PurchaseOrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_PurchaseOrder",
                schema: "public",
                table: "Fin_PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "SisaPembayaran",
                schema: "public",
                table: "Fin_DetailInvoiceReceived");

            migrationBuilder.RenameTable(
                name: "Fin_ARHeader",
                schema: "public",
                newName: "FIN_ARHeader",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_ARDokumen",
                schema: "public",
                newName: "FIN_ARDokumen",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_ARDetail",
                schema: "public",
                newName: "FIN_ARDetail",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_ARCanceled",
                schema: "public",
                newName: "FIN_ARCanceled",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_PurchaseOrderItem",
                schema: "public",
                newName: "PurchaseOrderItem",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_PurchaseOrder",
                schema: "public",
                newName: "PurchaseOrder",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Fin_PurchaseOrderItem_PurchaseOrderId",
                schema: "public",
                table: "PurchaseOrderItem",
                newName: "IX_PurchaseOrderItem_PurchaseOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_ARHeader",
                schema: "public",
                table: "FIN_ARHeader",
                column: "ARHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_ARDokumen",
                schema: "public",
                table: "FIN_ARDokumen",
                column: "ARDokumenId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_ARDetail",
                schema: "public",
                table: "FIN_ARDetail",
                column: "ARDetailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_ARCanceled",
                schema: "public",
                table: "FIN_ARCanceled",
                column: "ARCanceledId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrderItem",
                schema: "public",
                table: "PurchaseOrderItem",
                column: "PurchaseOrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrder",
                schema: "public",
                table: "PurchaseOrder",
                column: "PurchaseOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItem_PurchaseOrder_PurchaseOrderId",
                schema: "public",
                table: "PurchaseOrderItem",
                column: "PurchaseOrderId",
                principalSchema: "public",
                principalTable: "PurchaseOrder",
                principalColumn: "PurchaseOrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
