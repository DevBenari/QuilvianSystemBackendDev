using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class LaporanBedah : UserActivity
    {
        [Key]
        public Guid LaporanBedahId { get; set; }

        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? TindakanId { get; set; }
        public string? DetailTindakan { get; set; }
        public Guid? DokterOperatorId { get; set; }
        public Guid? DokterAnestesiId { get; set; }
        public Guid? DokterAsistenId { get; set; }
        public Guid? AsistenAnestesiId { get; set; }
        public Guid? PerawatId { get; set; }

        public string? JenisAnestesi { get; set; }
        public string? DiagnosaPraOp { get; set; }
        public string? DiagnosaPostOp { get; set; }
        public string? JaringanEksisiInsisi { get; set; }
        public string? TipeUrgensi { get; set; }
        public bool? IsPemeriksaanPA { get; set; }

        public DateTime? TanggalOperasi { get; set; }
        public DateTime? WaktuMulaiOperasi { get; set; }
        public DateTime? WaktuSelesaiOperasi { get; set; }
        public TimeSpan? DurasiOperasi { get; set; }

        public string? LaporanOperasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
