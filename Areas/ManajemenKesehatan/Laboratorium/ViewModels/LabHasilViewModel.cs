namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? LabBookingId { get; set; }
        public List<Guid>? UserActiveId { get; set; } = new List<Guid>();
        public Guid? PenanggungJawabId { get; set; }
        public Guid? PenanggungJawabAnalisId { get; set; }
        // specimen
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
        public DateTime? TanggalPemeriksaan { get; set; }
        public string? KategoriPemeriksaanPA { get; set; }
        public string? JenisPemeriksaan { get; set; }
        public string? LokasiSpecimen { get; set; }
        public string? KeteranganKlinis { get; set; }
        public string? JenisSpecimen { get; set; }
        public string? MasaHaidTerakhir { get; set; }
        public string? DiagnosaKlinis { get; set; }
        public string? RiwayatPenyakit { get; set; }
        public string? FiksasiDigunakan { get; set; }
        public string? PolaTujuanPengambilan { get; set; }
        public string? BahanNonGinekologi { get; set; }
        public string? Keterangan { get; set; }

    }
}
