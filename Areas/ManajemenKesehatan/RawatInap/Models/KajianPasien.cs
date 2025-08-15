using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("KajianPasien", Schema = "public")]
    public class KajianPasien : UserActivity
    {
        [Key]
        public Guid KajianPasienId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? UserActiveId { get; set; }
        public string? KeadaanUmum { get; set; }
        public string? KeadaanKulit { get; set; }
        public string? KeadaanKepalaLeher { get; set; }
        public string? KeadaanDada { get; set; }
        public string? KeadaanJantung { get; set; }
        public string? KeadaanParuParu { get; set; }
        public string? KeadaanAbdomen { get; set; }
        public string? KeadaanGenitalia { get; set; }
        public string? KeadaanAnggotaGerak { get; set; }
        public string? KeadaanLainnya { get; set; }
        public string? StatusLokalis { get; set; }
        public string? PemeriksaanPenunjang { get; set; }
        public string? DiagnosaSaatIni { get; set; }
        public string? DiagnosaBanding { get; set; }
        public string? DaftarMasalah { get; set; }
        public string? Program { get; set; }
        public string? Terapi { get; set; }
        public bool? Edukasi { get; set; }
        public string? EdukasiKepada { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglKajian { get; set; }
        public string? KajianUtamaPengkajian { get; set; }
        public Guid? CurrentMedicationId { get; set; }
    }
}
