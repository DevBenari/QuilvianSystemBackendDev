using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_RiwayatSertifikat", Schema = "public")]
    public class RiwayatSertifikat : UserActivity
    {
        [Key]
        public Guid SertifikasiId { get; set; }

        [Required]
        public Guid UserActiveId { get; set; }

        [Required]
        public string NamaSertifikasi { get; set; }

        [Required]
        public string NamaInstitusi { get; set; }

        [Required]
        public string Penyelenggara { get; set; }

        public long NoSertifikasi { get; set; }

        [Column(TypeName = "date")]
        public DateTime TglTerbit { get; set; }

        [Column(TypeName = "date")]
        public DateTime TglKadaluarsa { get; set; }

        public string AsalPartisipasi { get; set; }

        public string? FilePath { get; set; } // File path untuk upload file sertifikat
    }
}
