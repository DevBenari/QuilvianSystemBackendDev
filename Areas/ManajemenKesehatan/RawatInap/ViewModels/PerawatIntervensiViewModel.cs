namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PerawatIntervensiViewModel
    {
        public Guid? DiagnosaSDKIId { get; set; }
        public string? NamaIntervensi { get; set; }
        public string? TipeIntervensi { get; set; } // e.g., Hasil, Observasi, Terapeutik, Edukasi, Kolaborasi
        public string? Keterangan { get; set; }
    }
}
