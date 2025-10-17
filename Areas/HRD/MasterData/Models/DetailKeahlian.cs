using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_DetailKeahlian", Schema = "public")]
    public class DetailKeahlian : UserActivity
    {
        [Key]
        public Guid DetailKeahlianId { get; set; }
        public Guid? UserActiveId { get; set; }
        public Guid? KeahlianId { get; set; }

        [MaxLength(100)]
        public string? LevelKeahlian { get; set; }

        public Guid? Penilai { get; set; }
    }
}
