using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstResepDetail", Schema = "public")]
    public class ResepDetail : UserActivity
    {
        [Key]
        public Guid DetailResepId { get; set; }
        public Guid? ResepId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? RacikanId { get; set; }
        public bool? IsRacikan { get; set; } // "Ya" or "Tidak"
        public string? NamaAsuransi { get; set; }
        public Guid? ObatId { get; set; }
        public int? Qty { get; set; }
        public decimal? TakaranDosis { get; set; }
        public string? JenisRacikan { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? JenisObat { get; set; }
        public decimal? HargaObat { get; set; }
        public decimal? TotalHargaObat { get; set; }
        public bool? StatusCoverObat { get; set; } = false;
        public bool? StatusPengambilanObat { get; set; } = false;
        public bool? IsIteratur { get; set; } = false;
        public bool? IsContinued { get; set; } = false;
        public bool? StatusDiberikanPasien { get; set; } 
        public bool? IsObatDibawaPlg {  get; set; }
        public decimal? JumlahIteratur { get; set; }
        public string? EstimasiPemberian { get; set; }
        public string? CaraPemakaian { get; set; }
        public DateTime? TglStopPemakaian { get; set; }
        public DateTime? TglMulaiIteratur { get; set; }
        public decimal? JarakPenebusan { get; set; }
        public DateTime? MasaAktifIteratur { get; set; }
        public bool? ObatPagiDiambil { get; set; }
        public bool? ObatSiangDiambil { get; set; }
        public bool? ObatMalamDiambil { get; set; }

        // buat return obat
        public bool? IsReturn {  get; set; }
        public decimal? QtyReturn { get; set; }
        public string? AlasanReturn { get; set; }
        public Guid? DikembalikanOleh { get; set; }
    }
}
