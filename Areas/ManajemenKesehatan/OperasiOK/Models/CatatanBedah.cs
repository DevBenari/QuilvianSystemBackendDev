using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class CatatanBedah : UserActivity
    {
        [Key]
        public Guid CatBedahId { get; set; } // Generate Otomatis
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? DokterOperatorId { get; set; }
        public Guid? AsistenDokterId { get; set; }
        public Guid? DokterAnestesiId { get; set; }
        public Guid? AsistenAnestesiId { get; set; }
        public Guid? PerawatId { get; set; }
        public Guid? TindakanId { get; set; }
        public Guid? IcdPraOperasiId { get; set; }
        public string? DiagnosaPraOperasi { get; set; }
        public Guid? IcdPostOperasiId { get; set; }
        public string? DiagnosaPostOperasi { get; set; }
        public string? JenisOperasi { get; set; }
        public string? UrgensiOperasi { get; set; }
        public string? MacamOperasi { get; set; }
        public DateTime? TanggalOperasi { get; set; }
        public decimal? Jumlah { get; set; } // Tipe numeric dipetakan ke decimal
        public DateTime? WaktuMulaiOperasi { get; set; }
        public DateTime? WaktuSelesaiOperasi { get; set; }
        public TimeOnly? WaktuTambahan { get; set; }
        public TimeSpan? LamaOperasi { get; set; }
        public decimal? JumlahPendarahan { get; set; }
        public bool? IsJaringan { get; set; }
        public string? JenisJaringan { get; set; }
        public bool? IsPA { get; set; } // Pemeriksaan PA/Kultur
        public string? Komplikasi { get; set; }
        public string? CatatanSaatOperasi { get; set; } // Tipe text
        public string? PathTTDDokterOperator { get; set; }
    }
}

