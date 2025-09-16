namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class PermintaanUnitViewModel
    {
        public Guid? UnitId { get; set; }
        public string? JenisPermintaan { get; set; }
        public string? TglPembuatanPermintaan { get; set; }
        public string? StatusPermintaan { get; set; }
        public string? Keterangan { get; set; }
    }
}
