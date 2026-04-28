using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObatAlkes", Schema = "public")]
    public class ObatAlkes : UserActivity
    {
        [Key]
        public Guid? ObatAlkesId { get; set; } // Generate otomatis, biasanya dipakai saat update
        public string? KodeObatAlkes { get; set; }
        public Guid? GroupObatAlkesId { get; set; }
        [Required(ErrorMessage = "Nama obat/alkes wajib diisi")]
        public string? NamaObatAlkes { get; set; }
        public Guid? KategoriTerapeutikId { get; set; }
        public Guid? SubKategoriTerapeutikId { get; set; }
        public Guid? JenisObatId { get; set; }
        public bool? HighAlert { get; set; } = false;
        public Guid? SatuanId { get; set; }
        public decimal? Dosis { get; set; }
        public string? Etiket { get; set; }
        // Isi hanya: "Biru" atau "Putih"
        public Guid? KodeKFAId { get; set; }
        public string? BZA { get; set; }
        public string? POV { get; set; }
        public string? POAK { get; set; }
        public Guid? ObatRuteId { get; set; }
        public decimal? KekuatanSediaan { get; set; }
        public decimal? VolumeSediaan { get; set; }
        public decimal? BentukSediaan { get; set; }
        public Guid? KomoditasId { get; set; }
        public Guid? MaterialGroupId { get; set; }
        public decimal? StockMinimal { get; set; }
        public decimal? StockMaximal { get; set; }
        public Guid? BentukObatAlkesId { get; set; }
        public Guid? GolonganObatAlkesId { get; set; }
        public string? Keterangan { get; set; }
    }
}
