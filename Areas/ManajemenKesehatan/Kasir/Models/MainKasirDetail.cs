using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("MainKasirDetail", Schema = "public")]
    public class MainKasirDetail : UserActivity
    {
        [Key]
        public Guid MainKasirDetailId { get; set; }
        public Guid? MainKasirId { get; set; }
        public Guid? MetodePembayaranId { get; set; }
        public Guid? ReferenceId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? TotalPembayaran { get; set; }
        public decimal? SisaPembayaran { get; set; }
        public string? NoKwitansi {  get; set; }
        public decimal? AngsuranKe {  get; set; }
        public string? NamaMetode { get; set; }
        public decimal? NominalPembayaran { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglPembayaran { get; set; }
    }
}
