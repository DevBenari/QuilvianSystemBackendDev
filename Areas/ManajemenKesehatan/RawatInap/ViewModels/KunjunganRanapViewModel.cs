namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class KunjunganRanapViewModel
    {
        public Guid? PasienId { get; set; }
        public Guid? DokterDPJPId { get; set; }
        public string? TipePembayaran { get; set; }
        public bool? StatusRanap { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? SuratPengantarId { get; set; }
        public Guid? BedId { get; set; }
        public string? KeteranganSelesaiRanap { get; set; }
        public DateTime? TglAdministrasi { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? Keterangan { get; set; }
    }
}
