namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabBookingDetailViewModel
    {
        public Guid? BookingLabId { get; set; } // Relasi ke tabel Booking Lab
        public Guid? PasienId { get; set; } // Relasi ke tabel Pasien
        public Guid? PemeriksaanLabId { get; set; } // Relasi ke tabel Pemeriksaan Lab
        public Guid? LabId { get; set; } // Relasi ke tabel Lab
        public List<Guid>? SpecimenJenisId { get; set; }
        public List<Guid>? SpecimenMethodId { get; set; }
        public Guid? AsalSpecimenId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? KategoriPatologiAnatomi { get; set; } // Histological / Cytology / Non Gynae Cytology
        public string? JenisSpecimen { get; set; } // Biopsi / Operasi / Kerokan / Cairan Tubuh, dll
        public string? LokasiSpecimen { get; set; } // Lokasi pengambilan specimen
        public string? KeteranganKlinik { get; set; } // Catatan klinis
        public string? PerkiraanPenyakit { get; set; } // Diagnosa atau dugaan penyakit
        public string? PenyakitSebelumnya { get; set; } // Riwayat penyakit sebelumnya
        public string? PenggunaanFiksasi { get; set; } // Bahan atau metode fiksasi yang digunakan
        public string? JenisPemeriksaanGC { get; set; } // GC = Gynaecological Cytology
        public string? JenisGC { get; set; } // Jenis GC
        public string? BahanNonGC { get; set; } // Non-Gynaecological Cytology
        public string? BahanMicrobiologi { get; set; } // Jenis specimen mikrobiologi (urine, wound, respiratory, dll)
        public string? MasaHaidTerakhir { get; set; } // Informasi masa haid terakhir (jika relevan)
        public string? Diagnosa { get; set; }
        public decimal? Satuan { get; set; }
        public string? StatusPemeriksaan { get; set; }
        public DateTime? TanggalSelesai { get; set; }
        public bool? StatusVerifikasi { get; set; }
        public string? AlasanPembatalan {  get; set; }
    }
}
