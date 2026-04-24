using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstSoal", Schema = "public")]
    public class MasterSoal : UserActivity
    {
        [Key]
        public Guid SoalId { get; set; }

        [MaxLength(2000)]
        public string? Soal { get; set; }

        [MaxLength(200)]
        public string? KategoriTest { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
