using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_DokumenDetailKaryawan", Schema = "public")]
    public class DokumenDetailKaryawan : UserActivity
    {
        [Key]
        public Guid DokDetailId { get; set; }
        public Guid? UserActiveId { get; set; }

        [MaxLength(200)]
        public string? NamaPeserta { get; set; }

        [MaxLength(100)]
        public string? NoPeserta { get; set; }

        public DateTimeOffset? TglUpload { get; set; }

        [MaxLength(255)]
        public string? NamaDokumen { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? StatusKepemilikan { get; set; }
    }
}
