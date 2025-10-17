namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class EvaluasiAwalDetailViewModel
    {
        public Guid? EvaluasiAwalId { get; set; }         // Relasi dengan tabel EvaluasiAwal
        public Guid? ChecklistItemId { get; set; }        // Relasi dengan tabel ChecklistItem
        public string? Keterangan { get; set; }
        public string? TglPenyimpanan { get; set; }
    }
}
