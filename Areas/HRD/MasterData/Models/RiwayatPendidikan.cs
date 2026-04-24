using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_RiwayatPendidikan", Schema = "public")]
    public class RiwayatPendidikan : UserActivity
    {
        [Key]
        public Guid PendidikanId { get; set; }

        public Guid UserActiveId { get; set; }

        [Required]
        public string JenjangPendidikan { get; set; }

        [Required]
        public string NamaInstitusi { get; set; }

        [Required]
        public string Jurusan { get; set; }

        public int TahunMasuk { get; set; }

        public int TahunLulus { get; set; }

        [Column(TypeName = "decimal(4, 2)")]
        public decimal NilaiIpk { get; set; }

        public Guid ProvinsiId { get; set; }
    }
}
