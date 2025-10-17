namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class DetailKetergantunganViewModel
    {
        public Guid? KunjunganId { get; set; }           
        public Guid? PengkajianPerawatId { get; set; }   
        public Guid? KetergantunganId { get; set; }
        public Guid[]? IndikatorPengkajianIds { get; set; }
        public string? Keterangan { get; set; }
    }
}
