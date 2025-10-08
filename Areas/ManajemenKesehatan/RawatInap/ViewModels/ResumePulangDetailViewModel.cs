namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ResumePulangDetailViewModel
    {
        public Guid? ResumePulangId { get; set; }      // Relasi dengan tabel ResumePulang
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? PemakaianWc {  get; set; }
        public bool? Is65th { get; set; }
        public bool? IsPercobaanBunuhDiri { get; set; }
        public bool? IsKorbanKriminal { get; set; }
        public bool? IsKeterbatasanMobilitas { get; set; }
        public bool? IsPerawatanLanjutan { get; set; }   // Perawatan/Pengobatan Lanjutan
        public bool? IsBantuanADL { get; set; }          // Bantuan Aktivitas Sehari-Hari
        public string? TransportasiPulang { get; set; }
        public bool? IsPasienTinggalSendiri { get; set; }

        public string? NamaWali { get; set; }           // Wali yang merawat pasien setelah pulang
        public string? LetakKamarPasien { get; set; }
        public string? KondisiPenerangan { get; set; }
        public string? JarakKamarMandi { get; set; }
        public string? PerawatanYangDibantu { get; set; }

        public bool? IsDibantuAlatMedis { get; set; }    // Butuh alat medis setelah keluar RS
        public bool? IsAlatBantu { get; set; }           // Pasien menggunakan alat bantu setelah keluar RS
        public bool? IsPerluBantuanKhusus { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglDetailResumePulang { get; set; }

        public Guid? TTId { get; set; }                  // Perawat Id
        public IFormFile? TTDFile { get; set; }
    }
}
