using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasilDetail : UserActivity
    {
        [Key]
        public Guid DetailHasilLabId { get; set; }
        public Guid? HasilLabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? KelasId { get; set; }
        public  DateTime? TanggalSelesai { get; set; }
        public string? NoPhotoLab { get; set; }
        public string? PhotoLabPath { get; set; }
        public string? HasilLabManual { get; set; }
        public string? HasilLabAI {  get; set; }
        public string? JumlahFilm {  get; set; }
        public string? KeadaanSpecimen { get; set; }
        public Guid? AnalisId { get; set; }
        public bool? IsDefinitif { get; set; }
        public bool? IsDuplu { get; set; }
        public string? HasilMakroskopik { get; set; }
        public string? HasilMikroskopik { get; set; }
        public string? KesimpulanHasil { get; set; }
        public string? NilaiNormal { get; set; }
        public decimal? BloodVolume { get; set; }
        public decimal? SputumVolume { get; set; }
        public decimal? UrineVolume { get; set; }
        public decimal? PusVolume { get; set; }
        public decimal? StoolVolume { get; set; }
        public decimal? JaringanVolume { get; set; }
        public decimal? BodyFluidVolume { get; set; }
        public string? SatuanPemeriksaan { get; set; }
        public Guid? PetugasSpecimenId { get; set; }
        public DateTime? TanggalSpecimen {  get; set; }
        public TimeOnly? JamSpecimen {  get; set; }
        public string? InfoNReff {  get; set; }
        public string? Kondisi {  get; set; }
        public string? KategoriGC { get; set; }
        public string? Rincian {  get; set; }
        public string? Anjuran { get; set; }
        public string? DiagnosisPA { get; set; }
        public string? HasilImunoHistokimiaJson { get; set; }
        [JsonIgnore]
        public List<HasilImunoHistokimiaItem> HasilImunoHistokimia
        {
            get
            {
                if (string.IsNullOrWhiteSpace(HasilImunoHistokimiaJson))
                    return new List<HasilImunoHistokimiaItem>();

                return JsonSerializer.Deserialize<List<HasilImunoHistokimiaItem>>(
                           HasilImunoHistokimiaJson,
                           new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                       ) ?? new List<HasilImunoHistokimiaItem>();
            }
            set
            {
                HasilImunoHistokimiaJson = JsonSerializer.Serialize(
                    value ?? new List<HasilImunoHistokimiaItem>(),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );
            }
        }
        public string? Keterangan {  get; set; }
    }
}
