namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokterPoliViewModel
    {
        public Guid? PoliId { get; set; }
        public Guid DokterId { get; set; }
        public string NamaDokter { get; set; }
        public string? NamaPoliKlinik { get; set; }
    }
}
