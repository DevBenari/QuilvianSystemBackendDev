namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PengkajianKetergantunganViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PengkajianPerawatId { get; set; }
        public string? Mobilitas { get; set; }
        public string? Personal { get; set; }
        public string? Toileting { get; set; }
        public string? MakanMinum { get; set; }
        public string? Kesadaran { get; set; }
        public string? ObservasiTTV { get; set; }
        public string? Respirasi { get; set; }
        public string? Pengobatan { get; set; }
        public bool? IsLaporDPJP { get; set; }
        public string? AlatBantuADL { get; set; }
        public string? Keterangan { get; set; }
    }
}
