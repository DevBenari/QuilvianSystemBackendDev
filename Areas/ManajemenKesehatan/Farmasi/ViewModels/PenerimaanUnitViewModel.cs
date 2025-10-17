namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class PenerimaanUnitViewModel
    {
        public Guid? UnitId { get; set; }
        //public string? JenisPermintaan { get; set; }
        public string? TglPenerimaan { get; set; }
        public string? StatusPenerimaan { get; set; }
        public string? Keterangan { get; set; }
        public List<DetailPenerimaanUnitViewModel> DetailPenerimaanUnit { get; set; }
    }
}
