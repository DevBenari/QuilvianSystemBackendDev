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
        public decimal? NilaiNormal { get; set; }
        public decimal? BloodVolume { get; set; }
        public decimal? SputumVolume { get; set; }
        public decimal? UrineVolume { get; set; }
        public decimal? PusVolume { get; set; }
        public decimal? StoolVolume { get; set; }
        public decimal? JaringanVolume { get; set; }
        public decimal? BodyFluidVolume { get; set; }
        public Guid? PetugasSpecimenId { get; set; }
        public DateTime? TanggalSpecimen { get; set; }
        public TimeOnly? JamSpecimen { get; set; }
        public string? InfoNReff { get; set; }
        public string? Kondisi { get; set; }
        public string? KategoriGC { get; set; }
        public string? Rincian { get; set; }
        public string? Anjuran { get; set; }
        public string? DiagnosisPA { get; set; }
        public string? Keterangan { get; set; }
    }
}
