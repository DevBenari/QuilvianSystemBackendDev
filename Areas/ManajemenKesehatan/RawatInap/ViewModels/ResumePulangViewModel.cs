namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ResumePulangViewModel
    {
        public Guid? KunjunganId { get; set; }          // Relasi dengan tabel kunjungan
        public Guid? DokterId { get; set; }             // Relasi dengan tabel dokter
        public Guid? BookingBedId { get; set; }         // Relasi dengan tabel booking bed
        public Guid? DetailIcdid { get; set; }          // Detail ICD ID
        public string? IndikasiRanap { get; set; }      // Indikasi saat masuk ranap/ diagnosa awal
        public string? RiwayatPenyakit { get; set; }
        public string? PemeriksaanFisik { get; set; }
        public string? HasilLab { get; set; }
        public string? DiagnosaUtama { get; set; }
        public bool? IsOperasi { get; set; }            // Apakah pasien ada tindak operasi atau tidak
        public DateTime? WaktuKontrol { get; set; }
        public string? SaranPemeriksaan { get; set; }
        public Guid? ResepId { get; set; }
        public string? TerapiMedis { get; set; }        // Terapi selama di RS
        public string? HasilKonsultasi { get; set; }
        public bool? PendingResult { get; set; }
        public string? Diet { get; set; }
        public string? IsiEdukasi { get; set; }
        public string? KondisiPulang { get; set; }      // Keadaan pasien ketika pulang
        public string? TakeHomeResult { get; set; }    // Hasil pemeriksaan yang dibawa pulang
        public string? IntruksiPulang { get; set; }
        public string? TtdPenerima { get; set; }        // Image signature penerima
        public string? TtdPemberi { get; set; }         // Image signature pemberi
        public bool? StatusResume { get; set; }         // Diberikan / Belum diberikan
        public string? Keterangan { get; set; }
    }
}
