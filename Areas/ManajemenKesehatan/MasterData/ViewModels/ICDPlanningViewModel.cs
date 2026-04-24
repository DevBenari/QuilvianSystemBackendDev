using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ICDPlanningViewModel
    {
        public string? NamaPlanning { get; set; }
        public string? KategoriPlanning { get; set; }
        public string? Keterangan { get; set; }
        public string? DeskripsiDetail { get; set; }
    }
}
