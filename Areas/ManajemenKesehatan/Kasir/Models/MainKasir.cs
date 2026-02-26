using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("MainKasir", Schema = "public")]
    public class MainKasir : UserActivity
    {
        [Key]
        public Guid KasirId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? InvoiceBilling {  get; set; }
        public int? JumlahAngsuran {  get; set; }
        public string? StatusPembayaran { get; set; }
        public bool? IsVerified { get; set; }
        public Guid? TTDUserVerfiedId { get; set; }
        public string? PathUserVerified { get; set; }
        public decimal? JumlahPajak {  get; set; }
        public decimal? GrandTotalPembayaran { get; set; }
        public decimal? TotalBiayaObat { get; set; }
        public decimal? TotalBiayaTindakan { get; set; }
        public string? Keterangan { get; set; }
        public DateTimeOffset? TglPembayaran { get; set; }
        public Guid? DiskonId { get; set; }
        public decimal? SubTotalMandiri {  get; set; }
        public decimal? SubTotalAsuransi { get;  set; }
        public decimal? Deposito { get;  set; }
        public decimal? SisaDeposito { get; set; }
        public decimal? TotalPembayaran { get; set; }
    }

}
