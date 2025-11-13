namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabBookingViewModel
    {
        public Guid? KunjunganId { get; set; } // Relasi dengan tabel Kunjungan
        public Guid? PasienId { get; set; } // Relasi dengan tabel Pasien
        public Guid? AsuransiId { get; set; }
        public DateTime? TglPenyerahanSampling { get; set; } // Tanggal pengambilan atau penyerahan sampel
        public DateTime? TglBooking { get; set; } // Tanggal booking lab
        public DateTime? TglPemeriksaan { get; set; }
        public Guid? KelasId { get; set; } // Relasi ke tabel Kelas
        public Guid? DokterId { get; set; } // Relasi ke tabel Dokter
        public string? Keterangan { get; set; } // Catatan atau keterangan tambahan
        public bool? IsCito { get; set; } // Penanda apakah pemeriksaan bersifat "Cito" (darurat)
        public string? DiagnosaAwal { get; set; }
        public string? StatusPemeriksaan { get; set; }
        public Guid? DokterKonsulenId { get; set; }
        public Guid? TerapisId { get; set; }
        public decimal? HemodialisaKe { get; set; }
        public string? NomorSuratJaminan { get; set; }
        public string? CatatanJaminan { get; set; }
        public string? NoOrder { get; set; }
        public string? NoLab { get; set; }
        public string? NoPA { get; set; }
        public bool? StatusBookingLab { get; set; }
    }
}
