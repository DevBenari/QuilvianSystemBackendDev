using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models
{
    [Table("KasirTebusResep", Schema = "public")]
    public class KasirTebusResep : UserActivity
    {
        [Key]
        public Guid KasirTebusResepId { get; set; }
        public Guid? ResepTebusId { get; set; }
        public string? NoRegistrasi { get; set; }
        public decimal? NoAntrian { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public string? NamaMetode { get; set; }
        public bool? StatusPembayaran { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TanggalBayar { get; set; }

    }
}
