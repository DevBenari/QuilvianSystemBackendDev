using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstKeahlian", Schema = "public")]
    public class MasterKeahlian : UserActivity
    {
        [Key]
        public Guid KeahlianId { get; set; }

        [MaxLength(200)]
        public string? NamaKeahlian { get; set; }

        public bool? IsActive { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
