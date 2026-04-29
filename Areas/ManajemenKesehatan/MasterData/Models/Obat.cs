using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
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


        // Navigation ke master
        public Satuan? Satuan { get; set; }
        public BentukObat? BentukObat { get; set; }

        // icollection ke transaksi
        public ICollection<DetailPermintaanUnit> DetailPermintaanUnits { get; set; } = new List<DetailPermintaanUnit>();
        public ICollection<DetailPenerimaanUnit> DetailPenerimaanUnits { get; set; } = new List<DetailPenerimaanUnit>();
        public ICollection<FarmasiRJ> FarmasiRJs { get; set; } = new List<FarmasiRJ>();
        public ICollection<ObatReturnDetail> ObatReturnDetails { get; set; } = new HashSet<ObatReturnDetail>();

        public ICollection<RacikanDetail> RacikanDetails { get; set; } = new HashSet<RacikanDetail>();
        public ICollection<ResepDetail> ResepDetails { get; set; } = new HashSet<ResepDetail>();
    }
}
