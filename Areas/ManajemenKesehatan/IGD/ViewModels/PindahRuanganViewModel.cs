namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class PindahRuanganViewModel
    {
        public Guid? UnitId { get; set; }
        public Guid? KamarId { get; set; }
        public DateTime? TglAwal { get; set; }
        public DateTime? TglAkhir { get; set; }
        public string? Keterangan { get; set; }
    }
}
