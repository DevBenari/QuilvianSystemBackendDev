namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class HandoverPasienDetailViewModel
    {
        public Guid? HandoverPasienId { get; set; }
        public Guid? ChecklistItemId { get; set; }
        public bool? IsSudah { get; set; }
        public string? Keterangan { get; set; }
    }
}
