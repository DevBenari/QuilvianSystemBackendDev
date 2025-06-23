namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepTebusViewModel
    {
        public List<ResepTebusDetailViewModel>? DaftarObat { get; set; }
        public string? StatusPembuatanResep { get; set; }
        public DateOnly? TanggalPembuatanResep { get; set; }
    }
}
