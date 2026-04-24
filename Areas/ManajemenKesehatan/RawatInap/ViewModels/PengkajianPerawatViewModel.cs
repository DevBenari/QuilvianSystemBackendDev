namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PengkajianPerawatViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PendaftaranPasienBaruId { get; set; }
        public Guid? DokterId { get; set; }
        public string? SumberData { get; set; }
        public string? HubunganDenganPasien { get; set; }
        public string? TglMasuk { get; set; }
        public string? TglPengkajianPerawat { get; set; }
        public string? MasalahPsikologi { get; set; }
        public bool? IsHubunganSosial { get; set; }
        public string? TempatTinggal { get; set; }
        public string? GangguanFungsional { get; set; }
        public string? NilaiKepercayaan { get; set; }
        public DateTime? MensPertama { get; set; }
        public DateTime? MensTerakhir { get; set; }
        public decimal? Minum { get; set; }
        public string? TipeImunisasi { get; set; }
        public DateTime? TanggalImunisasiLanjutan { get; set; }
        public string? Keterangan { get; set; }
    }
}
