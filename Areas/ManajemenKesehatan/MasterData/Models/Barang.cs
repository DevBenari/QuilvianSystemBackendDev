using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstBarang", Schema = "public")]
    public class Barang : UserActivity
    {
        [Key]
        public Guid? BarangId { get; set; }

        // Ambil 3 huruf dari grup kategori barang + 0001
        public string? KodeBarang { get; set; }

        // ID dari table lain (misal: Obat)
        public Guid? ItemId { get; set; }

        public string? NamaBarang { get; set; }
        public Guid? KategoriBarangId { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? KelasResikoId { get; set; }
        public string? Spesifikasi { get; set; }
        public bool? IsPerluResep { get; set; }
        public decimal? StokMinimum { get; set; }
        public decimal? StokMaximum { get; set; }
        public string? Keterangan { get; set; }
    }
}
