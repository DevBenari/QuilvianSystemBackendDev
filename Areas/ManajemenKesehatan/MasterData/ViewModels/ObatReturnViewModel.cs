namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ObatReturnViewModel
    {
        public Guid? KasirId { get; set; }
        public Guid? ReferenceId { get; set; }
        public bool? StatusPembayaran { get; set; }
        public string? Keterangan { get; set; }
    }
}
