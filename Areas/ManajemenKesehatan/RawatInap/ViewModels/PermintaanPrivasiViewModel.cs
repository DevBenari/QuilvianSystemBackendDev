namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PermintaanPrivasiViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? AksesDiperbolehkan { get; set; }
        public string? PermintaanKhusus { get; set; }
        public bool? IsTransportasiPrivasi { get; set; }
        public DateTime? TanggalPermintaan { get; set; }
        public Guid? KepalaRuanganId { get; set; }
        public IFormFile? TTDPenandaTangan { get; set; }
        public string? Keterangan { get; set; }
    }
}
