using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class CoveranAsuransiViewModel
    {
        public string NamaAsuransi { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceDesc { get; set; }
        public string ServiceCodeClass { get; set; }
        public string Class { get; set; }
        public bool IsSurgery { get; set; }
        public int Tarif { get; set; }
        public DateTime? TglBerlaku { get; set; }
        public DateTime? TglBerakhir { get; set; }
        public bool? IsPKS { get; set; }
        public Guid? AsuransiId { get; set; }
    }
}
