using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class CoveranObatAsuransiViewModel
    {
        public string? NamaAsuransi { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceDesc { get; set; }
        public string? ServiceCodeClass { get; set; }
        public string? Class { get; set; }
        public bool? IsSurgery { get; set; }
        public decimal? Tarif { get; set; }
        public string? TglBerlaku { get; set; }
        public string? TglBerakhir { get; set; }
        public bool? IsPKS { get; set; }
        public Guid? AsuransiId { get; set; }
    }
}
