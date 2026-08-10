using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasil : UserActivity
    {
        [Key]
        public Guid HasilLabId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? LabBookingId { get; set; }
        public Guid? DokterPerujukId { get; set; }
        public Guid? DokterKonfirmatorId { get; set; }
        public string? NoPhoneKonfirmator {  get; set; }
        public bool? IsKonfirmatorDPJP {  get; set; }
        public List<Guid>? UserActiveId { get; set; } = new List<Guid>();
        public Guid? PenanggungJawabId { get; set; }
        public Guid? PenanggungJawabAnalisId { get; set; }
        public DateTime? TanggalPemeriksaan {  get; set; }
        public string? DokterLuarRS { get; set; }
        public string? Keterangan {  get; set; }

        // specimen
        public decimal? BloodVolume { get; set; }
        public decimal? SputumVolume { get; set; }
        public decimal? UrineVolume { get; set; }
        public decimal? PusVolume { get; set; }
        public decimal? StoolVolume { get; set; }
        public decimal? JaringanVolume { get; set; }
        public decimal? BodyFluidVolume { get; set; }
        public string? BahanPemeriksaanLainnya { get; set; }
        public string? KeteranganBahanPemeriksaan { get; set; }
        public Guid? PetugasSpecimenId { get; set; }
        public DateTime? TanggalSpecimen { get; set; }
        public TimeOnly? JamSpecimen { get; set; }

        // =====================================
        // DATA PATOLOGI ANATOMI
        // =====================================
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

        // wa message ke pasien
        public StatusKirimHasilWhatsapp? StatusKirimHasil { get; set; } = StatusKirimHasilWhatsapp.BelumDikirim;
        public int? JumlahKirimHasil { get; set; } = 0;
        public DateTimeOffset? TanggalKirimHasilTerakhir { get; set; }

        // navigatiom
        public Kunjungan? Kunjungan { get; set; }
        public Lab? Lab { get; set; }
        public LabBooking? LabBooking { get; set; }
        public Dokter? DokterPerujuk { get; set; }
        public Dokter? DokterKonfirmator { get; set; }
        public UserActive? PenanggungJawab { get; set; }
        public UserActive? PenanggungJawabAnalis {  get; set; }
    }
}
