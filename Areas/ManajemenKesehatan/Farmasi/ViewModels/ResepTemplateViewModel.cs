using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepTemplateViewModel
    {
        public string? Judul { get; set; }
        public Guid? DokterId { get; set; }
        public List<ResepTemplateDetailViewModel>? DaftarObat { get; set; }
    }
}
