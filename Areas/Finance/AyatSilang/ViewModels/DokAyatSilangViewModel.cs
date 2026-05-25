namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.ViewModels
{
    public class DokAyatSilangViewModel
    {

        public Guid AyatSilangId { get; set; }

        public string NamaDokumen { get; set; }

        public IFormFile FileAyatSilang { get; set; }
        public string? FilePath { get; set; }

        public DateTime TglPenyimpanan { get; set; }

        public string Keterangan { get; set; }
    }
}
