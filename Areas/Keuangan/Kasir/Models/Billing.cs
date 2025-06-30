using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models
{
    [Table("Billing", Schema = "public")]
    public class Billing : UserActivity
    {
        [Key]
        public Guid BillingId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? DiskonId { get; set; }
        public DateTime? BillingDate { get; set; }
        public string? BillingKode { get; set; }
        public Guid? ItemId { get; set; }
        public string? NamaItem { get; set; }
        public decimal? HargaItem { get; set; }
        public decimal? SubTotalItem { get; set; }
        public string? Keterangan { get; set; }
    }
}
