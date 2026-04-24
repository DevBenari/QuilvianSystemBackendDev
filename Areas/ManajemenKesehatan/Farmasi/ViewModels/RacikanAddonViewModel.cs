namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class RacikanAddonViewModel
    {
        public Guid? BentukSatuanId { get; set; }
        public string? NamaBentukSatuan { get; set; }
        public decimal? BiayaJasaRacikan { get; set; }
        public decimal? BiayaKemasanRacikan { get; set; }
        public string? Keterangan { get; set; }
    }
}
