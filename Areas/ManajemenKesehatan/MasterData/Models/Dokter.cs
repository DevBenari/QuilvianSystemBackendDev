using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokter", Schema = "public")]
    public class Dokter : UserActivity
    {
        [Key]
        public Guid DokterId { get; set; }
        public string KdDokter { get; set; }
        public string NmDokter { get; set; }
        public string Sip { get; set; }
        public string Str { get; set; }
        public DateTime? TglSip { get; set; }
        public DateTime? TglStr { get; set; }
        public string PanggilDokter { get; set; }
        public string Nik { get; set; }
    }
}
