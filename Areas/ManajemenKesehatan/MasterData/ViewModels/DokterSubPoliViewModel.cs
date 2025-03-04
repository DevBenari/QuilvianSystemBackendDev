namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokterSubPoliViewModel
    {
        public Guid? SubPoliId { get; set; }
        public Guid DokterId { get; set; }
        public string NamaDokter { get; set; }
        public string? NamaPoliKlinik { get; set; }
        public string? NamaSubPoli { get; set; }
    }
}
