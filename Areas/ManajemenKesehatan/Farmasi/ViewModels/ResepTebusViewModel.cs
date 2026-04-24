namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepTebusViewModel
    {
        public string? NamaPenebus { get; set; }
        public List<ResepTebusDetailViewModel>? DaftarObat { get; set; }
        public string? StatusPembuatanResep { get; set; }
    }
}
