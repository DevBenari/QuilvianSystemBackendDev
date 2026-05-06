namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class RacikanViewModel
    {
        public string? NamaRacikan { get; set; }
        public string? Keterangan { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public int? QtyRacikan { get; set; }
        public Guid? BentukRacikanId { get; set; }

        public List<RacikanDetailViewModel>? DaftarRacikan { get; set; }
    }
}
