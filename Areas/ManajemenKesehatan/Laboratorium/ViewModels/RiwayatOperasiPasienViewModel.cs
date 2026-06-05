namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class RiwayatOperasiPasienViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? SumberDataId { get; set; }
        public string? NamaSumberData { get; set; }
        public string? NamaOperasi { get; set; }
        public string? LokasiTubuh { get; set; }
        public string? IndikasiOperasi { get; set; }
        public DateTime? WaktuOperasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
