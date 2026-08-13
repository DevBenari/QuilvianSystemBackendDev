using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabBooking : UserActivity
    {
        [Key]
        public Guid BookingLabId { get; set; } // Generate Otomatis
        public Guid? KunjunganId { get; set; } // Relasi dengan tabel Kunjungan
        public Guid? PasienId { get; set; } // Relasi dengan tabel Pasien
        public Guid? AsuransiId { get; set; }
        public DateTime? TglSampling { get; set; } // Tanggal pengambilan atau penyerahan sampel
        public DateTime? TglBooking { get; set; } // Tanggal booking lab
        public DateTime? TglPemeriksaan { get; set; }
        public Guid? KelasId { get; set; } // Relasi ke tabel Kelas
        public Guid? DiskonId { get; set; } 

        // Dokter
        public Guid? DokterKonsulenId { get; set; }
        public Guid? TerapisId { get; set; }
        public Guid? DokterPerujukId { get; set; }
        public Guid? DokterPemeriksaId { get; set; } // Relasi ke tabel Dokter

        public Guid? KonfirmatorId { get; set; }
        public string? Keterangan { get; set; } // Catatan atau keterangan tambahan
        public string? DiagnosaAwal { get; set; }
        public string? StatusPemeriksaan { get; set; }
        public decimal? HemodialisaKe { get; set; }
        public string? NomorSuratJaminan{get; set;}
        public string? CatatanJaminan {  get; set; }
        public string? NoOrder {  get; set; }
        public string? NoLab {  get; set; }
        public string? NoPA { get; set; }
        public bool? StatusBookingLab { get; set; }
        public string? IsLunas { get; set; }
        public bool? IsCito { get; set; } // Penanda apakah pemeriksaan bersifat "Cito" (darurat)
        public bool? IsPasienPersiapan { get; set; }
        public List<string>? Specimen { get; set; } = new();
        public bool? IsSpecimenLayak { get; set; }
        public bool? SuratRujukan { get; set; }
        public decimal? KalkulasiTercover { get; set; }
        public decimal? KalkulasiTidakTercover { get; set; }
        public string? AlasanPembatalan { get; set; }
        public string? TTDPathPembatalan { get; set; }
        public string? PetugasPembatalan { get; set; }
        public string? StatusKonfirmasi { get; set; }
        public string? ProsesBooking {  get; set; }
        public string? TindakLanjut {  get; set; }
        public string? HasilPenunjangLab { get; set; }
        public string? AnjuranDiet { get; set; }
        public TimeOnly? WaktuKonfirmasi { get; set; }
        public DateTime? TglKonfirmasi {  get; set; }
        
        // Tambahan baru
        public TimeOnly? WaktuPemeriksaan { get; set; }
        public TimeOnly? WaktuPemeriksaanPersiapan { get; set; }

        // Navigation
        public Kunjungan? Kunjungan { get; set; }
        public PendaftaranPasienBaru? Pasien { get; set; }
        public Asuransi? Asuransi { get; set; }
        public Kelas? Kelas { get; set; }
        public Dokter? DokterKonsulen { get; set; }
        public Dokter? DokterPerujuk { get; set; }
        public Dokter? DokterPemeriksa { get; set; }
        public UserActive? Konfirmator { get; set; }
        public Diskon? Diskon { get; set; }


        // Relasi paling penting
        public ICollection<LabBookingDetail> LabBookingDetails { get; set; } = new HashSet<LabBookingDetail>();
        public ICollection<LabBookingBatal> LabBookingBatals { get; set; } = new List<LabBookingBatal>();
        public ICollection<LabHasilBakteri> LabHasilBakteris { get; set; }
    = new List<LabHasilBakteri>();
    }
}
