using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class ResumePulangDetail : UserActivity
    {
        [Key]
        public Guid DetResumePulangId { get; set; }   // Generate Otomatis

        public Guid? ResumePulangId { get; set; }      // Relasi dengan tabel ResumePulang
        public Guid? PasienID { get; set; }
        public Guid? KunjunganId { get; set; }
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
        public bool? Status {  get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglDetailResumePulang { get; set; }
        public string? PemakaianWC {  get; set; }
        public Guid? PerawatId { get; set; }                  // Perawat Id
        public string? TTDPerawatPath { get; set; }

        // Optional: Navigation Properties (jika pakai EF Core relasi)
        // public virtual ResumePulang ResumePulang { get; set; }
        // public virtual UserActive UserActive { get; set; }
        // public virtual Perawat TT { get; set; }
    }
}
