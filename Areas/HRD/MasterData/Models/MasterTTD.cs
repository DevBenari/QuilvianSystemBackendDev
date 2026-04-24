using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstTTD", Schema = "public")]
    public class MasterTTD : UserActivity
    {
        [Key]
        public Guid TTDId { get; set; }
        public Guid? UserActiveId { get; set; }

        [MaxLength(500)]
        public string? TTDPath { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}
