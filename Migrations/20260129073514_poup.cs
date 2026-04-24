using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class poup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListPurchaseRequestId",
                schema: "public",
                table: "PurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "public",
                table: "PurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "PurchaseRequestId",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "TermOfPaymentId",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                schema: "public",
                table: "PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusPO",
                schema: "public",
                table: "PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                schema: "public",
                table: "PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TermOfPayment",
                schema: "public",
                table: "PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAccess",
                schema: "public",
                table: "PurchaseOrder",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItem_PurchaseOrderId",
                schema: "public",
                table: "PurchaseOrderItem",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItem_PurchaseOrder_PurchaseOrderId",
                schema: "public",
                table: "PurchaseOrderItem");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItem_PurchaseOrderId",
                schema: "public",
                table: "PurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "StatusPO",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "TermOfPayment",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "UserAccess",
                schema: "public",
                table: "PurchaseOrder");

            migrationBuilder.AddColumn<Guid>(
                name: "ListPurchaseRequestId",
                schema: "public",
                table: "PurchaseOrderItem",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                schema: "public",
                table: "PurchaseOrderItem",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseRequestId",
                schema: "public",
                table: "PurchaseOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "public",
                table: "PurchaseOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TermOfPaymentId",
                schema: "public",
                table: "PurchaseOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
