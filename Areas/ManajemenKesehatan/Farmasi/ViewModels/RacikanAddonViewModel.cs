namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class RacikanAddonViewModel
    {
        public Guid? BentukObatId { get; set; }
        public string? NamaBentukObat { get; set; }
        public decimal? BiayaJasaRacikan { get; set; }
        public decimal? BiayaKemasanRacikan { get; set; }
        public string? Keterangan { get; set; }
    }
}
