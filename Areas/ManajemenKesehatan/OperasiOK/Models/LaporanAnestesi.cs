using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class LaporanAnestesi : UserActivity
    {
        public Guid? LaporanAnestesiId { get; set; } // Generate Otomatis
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? TindakanId { get; set; }
        public string? DetailTindakan { get; set; }
        public Guid? DokterOperatorId { get; set; }
        public Guid? DokterAnestesiId { get; set; }
        public Guid? DokterAsistenId { get; set; }
        public Guid? AsistenAnestesiId { get; set; }
        public Guid? PerawatId { get; set; }
        public List<string>? Premidikasi { get; set; }
        public DateTime? TanggalOperasi { get; set; }
        public DateTime? WaktuSelesaiOperasi { get; set; }
        public DateTime? WaktuMulaiOperasi { get; set; }
        public TimeSpan? DurasiOperasi { get; set; }
        public DateTime? WaktuMulaiAnestesi { get; set; }
        public DateTime? WaktuSelesaiAnestesi { get; set; }
        public TimeSpan? DurasiAnestesi { get; set; }
        public string? PosisiOperasi { get; set; }
        public string? Oksigenasi { get; set; }
        public List<string>? Induksi { get; set; }
        public string? Intubasi { get; set; }
        public decimal NoIntubasi { get; set; }
        public string? ProsesIntubasi { get; set; }
        public string? AlasanProsesIntubasi { get; set; }
        public string? GenderBayiLahir { get; set; }
        public DateTime? WaktuCesar { get; set; }
        public decimal APGARScore { get; set; }
        public string? PathTTDDokterAnestesi { get; set; }
        public string? Keterangan { get; set; }
    }
}
