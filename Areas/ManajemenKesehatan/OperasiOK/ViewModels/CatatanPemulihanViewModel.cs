namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class CatatanPemulihanViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? DokterOperatorId { get; set; }
        public Guid? PerawatId { get; set; }
        public DateTime? WaktuMasuk { get; set; }
        public string? InfusTransfusi { get; set; }
        public decimal? JumlahUrine { get; set; }
        public string? Komplikasi { get; set; }
        public string? Penatalaksanaan { get; set; }
        public string? InfusSedasi { get; set; }
        public string? Antibiotika { get; set; }
        public string? Analgetik { get; set; }
        public string? AntiMuntah { get; set; }
        public string? Minum { get; set; }
        public string? PosisiPasien { get; set; }
        public string? Dipindahkan { get; set; }
        public DateTime? WaktuKeluar { get; set; }
        public string? PathDokterOperator { get; set; }
        public string? PathPerawat { get; set; }
        public string? Keterangan { get; set; }

        // details
        public List<CatatanPemulihanDetailViewModel>? Details { get; set; }
    }
}
