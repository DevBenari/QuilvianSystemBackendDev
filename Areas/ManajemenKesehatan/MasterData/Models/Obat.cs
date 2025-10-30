using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObat", Schema = "public")]
    public class Obat : UserActivity
    {
        [Key]
        public Guid ObatId { get; set; }
        public string? ObatCode { get; set; }
        public string ObatName { get; set; }
        public string? JumlahSatuan { get; set; }
        public Guid? SatuanId { get; set; }
        public Guid? BentukObatId { get; set; }
        public decimal HTEPrice { get; set; }
        public decimal? HNAPrice { get; set; }
        public bool? IsActive { get; set; }
        public int Stock { get; set; }
        public int? Minimal { get; set; }
        public int? Maximal { get; set; }
        public string? Farmakologi { get; set; }
        public string? Peringatan { get; set; }
        public string? Indikasi { get; set; }
        public string? Kontraindikasi { get; set; }
        public string? CaraKerja { get; set; }
        public string? InteraksiObat { get; set; }
        public decimal? TakaranDosis { get; set; }
        public string? Dosis { get; set; }
        public string? Note { get; set; }
        public decimal? Cogs { get; set; }
        public string? Kategori{ get; set; }
        public Guid? ItemId { get; set; }
        public Guid? ObatRuteId { get; set; }
        public string? KategoriObat { get; set; }
        public bool? IsControlled { get; set; }
        //public string Asuransi { get; set; }
        //public string KandunganObat { get; set; }
        //public string TipeHarga { get; set; }

    }
}
