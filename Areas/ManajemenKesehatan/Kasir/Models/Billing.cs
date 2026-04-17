using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("Billing", Schema = "public")]
    public class Billing : UserActivity
    {
        [Key]
        public Guid BillingId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? DiskonId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? AsuransiExcessId { get; set; }
        public DateTime? BillingDate { get; set; }
        public string? BillingKode { get; set; }
        public Guid? ItemId { get; set; }
        public string? TipeLayanan {  get; set; }
        public string? NamaItem { get; set; }
        public decimal? HargaItem { get; set; }
        public int? QtyItem { get; set; }
        public decimal? SubTotalItem { get; set; }
        public decimal? SubBiayaLainnya { get; set; }
        public string? Keterangan { get; set; }
        public string? JenisBilling { get; set; }
        public string? InvoiceBilling { get; set; }
        public bool? IsListWhiteOff { get; set; }
        public bool? StatusPengambilan { get; set; }
        public bool? StatusBilling {  get; set; }
        public bool? StatusBiayaLainnya {  get; set; }
        public bool? IsCovered {  get; set; }
        public bool? IsCoveredExcess {  get; set; }
        public DateTime? TanggalInvoice {  get; set; }
        public DateTime? TanggalJatuhTempo { get; set; }
        public int? DPD {  get; set; }
        public Guid? LayananId { get; set; }
        
    }
}
