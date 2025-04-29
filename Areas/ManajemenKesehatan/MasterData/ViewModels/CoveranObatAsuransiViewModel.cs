using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class CoveranObatAsuransiViewModel
    {
        public Guid? ObatId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? KategoriObatId { get; set; }
        public string? NamaKategoriObat { get; set; }
        public decimal? HargaRetail { get; set; }
        public string? NamaAsuransi { get; set; }
        public int? PersentaseDiskon { get; set; }
        public decimal? TarifObatAsuransi { get; set; }
        public bool? IsPKS { get; set; }
    }
}
