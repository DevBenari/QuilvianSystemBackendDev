using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilDetailViewModel
    {
        public Guid? HasilLabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? KelasId { get; set; }
        public DateTime? TanggalSelesai { get; set; }
        public List<IFormFile>? PhotoLab { get; set; }
        public string? HasilLabManual { get; set; }
        public string? HasilLabAI { get; set; }
        public string? JumlahFilm { get; set; }
        public string? KeadaanSpecimen { get; set; }
        public Guid? AnalisId { get; set; }
        public bool? IsDefinitif { get; set; }
        public bool? IsDuplu { get; set; }
        public string? HasilMakroskopik { get; set; }
        public string? HasilMikroskopik { get; set; }
        public string? KesimpulanHasil { get; set; }
        public string? NilaiNormal { get; set; }
        public string? SatuanPemeriksaan { get; set; }
        public string? InfoNReff { get; set; }
        public string? Kondisi { get; set; }
        public string? KategoriGC { get; set; }
        public string? Rincian { get; set; }
        public string? Anjuran { get; set; }
        public string? DiagnosisPA { get; set; }
        public List<HasilImunoHistokimiaItem>? HasilImunoHistokimia { get; set; } = new();
        public string? Keterangan { get; set; }
        public string? DetailDiagnosaKlinis { get; set; }
        public string? ReseptorEstrogenER { get; set; }
        public string? ReseptorProgesteronPR { get; set; }
        public string? HER { get; set; }
        public string? Ki67 { get; set; }
        public string? StatusER { get; set; }
        public string? StatusPR { get; set; }
        public string? HERImunohistokimia { get; set; }
        public string? StatusHasil { get; set; }
        public string? HasilPemeriksaan { get; set; }
        public string? LainLain { get; set; }
    }
}
