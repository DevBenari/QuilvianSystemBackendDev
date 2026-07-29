using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabBookingDetail : UserActivity
    {
        [Key]
        public Guid DetailBookingLabId { get; set; } // Generate Otomatis
        public Guid? BookingLabId { get; set; } // Relasi ke tabel Booking Lab
        public Guid? PasienId { get; set; } // Relasi ke tabel Pasien
        public Guid? PemeriksaanLabId { get; set; } // Relasi ke tabel Pemeriksaan Lab
        public Guid? LabId { get; set; } // Relasi ke tabel Pemeriksaan Lab
        public Guid? DokterPemeriksaId { get; set; } // Relasi ke tabel Dokter
        public List<Guid>?  SpecimenJenisId { get; set; }
        public List<Guid>?  SpecimenMethodId { get; set; }
        public Guid?  AsalSpecimenId { get; set; }
        public string? KategoriPatologiAnatomi { get; set; } // Histological / Cytology / Non Gynae Cytology
        public string? JenisSpecimen { get; set; } // Biopsi / Operasi / Kerokan / Cairan Tubuh, dll
        public string? LokasiSpecimen { get; set; } // Lokasi pengambilan specimen
        public string? KeteranganKlinik { get; set; } // Catatan klinis
        public string? PenyakitSebelumnya { get; set; } // Riwayat penyakit sebelumnya
        public string? PenggunaanFiksasi { get; set; } // Bahan atau metode fiksasi yang digunakan
        public string? JenisPemeriksaanGC { get; set; } // GC = Gynaecological Cytology
        public string? JenisGC { get; set; } // Jenis GC
        public string? BahanNonGC { get; set; } // Non-Gynaecological Cytology
        public string? BahanMicrobiologi { get; set; } // Jenis specimen mikrobiologi (urine, wound, respiratory, dll)
        public string? MasaHaidTerakhir { get; set; } // Informasi masa haid terakhir (jika relevan)
        public decimal? QtyOrder {  get; set; }
        public string? StatusPemeriksaan {  get; set; }
        public DateTime? TanggalSelesai { get; set; }
        public bool? StatusVerifikasi { get; set; }
        public string? AlasanPembatalan {  get; set; }
        public string? TTDPembatalanPath { get; set; }
        public string? TipeLayanan {  get; set; }
        public string? NoPhoto {  get; set; }

        // Navigation
        public LabBooking? LabBooking { get; set; }
        public Lab? Lab { get; set; }
        public PendaftaranPasienBaru? Pasien { get; set; }
        public LabPemeriksaan? PemeriksaanLab { get; set; }
        public SpecimenAsal? AsalSpecimen { get; set; }
        public Dokter? DokterPemeriksa { get; set; }

        public ICollection<LabBookingBatal> LabBookingBatals { get; set; } = new List<LabBookingBatal>();
    }
}
