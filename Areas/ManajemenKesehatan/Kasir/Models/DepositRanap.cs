using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    public class DepositRanap : UserActivity
    {
        [Key]
        public Guid DepositRanapId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? NoKwitansi {  get; set; }
        public DateTime? TglTransaksi { get; set; }
        public decimal? NominalMasuk { get; set; }
        public decimal? NominalKeluar { get; set; }
        public decimal? SaldoDeposit {  get; set; }
        public string? StatusDeposit { get; set; }
        public string? Keterangan { get; set; }
    }
}
