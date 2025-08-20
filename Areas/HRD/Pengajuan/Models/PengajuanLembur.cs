using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models
{
    [Table("Hrd_PengajuanLembur", Schema = "public")]
    public class PengajuanLembur : UserActivity
    {
        [Key]
        public Guid PengajuanLemburId { get; set; }
        public Guid UserActiveId { get; set; }
        public Guid DepartementId { get; set; }
        public Guid JenisLemburId { get; set; }
        [Column(TypeName = "date")] // hanya tanggal, tanpa jam
        public DateTime TglLembur { get; set; }
        public string? Keterangan { get; set; }
        public int LamaLembur { get; set; }
        public string? Deskripsi { get; set; }
        public Guid? ApprovedBy1 { get; set; }
        public Guid? ApprovedBy2 { get; set; }
    }
}
