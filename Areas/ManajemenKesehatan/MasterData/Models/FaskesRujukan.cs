using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstFaskesRujukan")]
    public class FaskesRujukan : UserActivity
    {
        [Key]
        public Guid FaskesRujukanId { get; set; }

        public string? NamaFaskesRujukan { get; set; }

        public string? AlamatFaskesRujukan { get; set; }

        public string? NoTelpFaskesRujukan { get; set; }

        public string? Keterangan { get; set; }

        // ============================
        // Navigation Property
        // ============================

        public virtual ICollection<LabRujukan>? LabRujukans { get; set; }
    }
}
