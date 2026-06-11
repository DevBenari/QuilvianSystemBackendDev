using QuilvianSystemBackendDev.Areas.Finance.Po.Models;

namespace QuilvianSystemBackendDev.Areas.Finance.Po.ViewModels
{
    public class PurchaseOrderViewModel
    {
        public string? PurchaseRequestNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? RequestType { get; set; }
        public string? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string? TermOfPayment { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public decimal? RemainingDay { get; set; }
        public decimal? QtyTotal { get; set; }
        public decimal? GrandTotal { get; set; }

        public string? UserAccess { get; set; }
        public string? StatusPO { get; set; }
        public string? Keterangan { get; set; }

        // 🔑 INIT supaya tidak null
        public List<PurchaseOrderItemViewModel> Items { get; set; } = new();
    }

}
