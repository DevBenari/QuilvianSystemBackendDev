using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokterSubPoli", Schema = "public")]
    public class DokterSubPoli : UserActivity
    {
        [Key]
        public Guid DokterSubPoliId { get; set; }
        public Guid DokterId { get; set; }
        public string NamaDokter { get; set; } 
        public string? KodeDokterSubPoli { get; set; }
        public Guid? SubPoliId { get; set; }
        public string? NamaSubPoli { get; set; }

        //relasi ke Asuransi
        [ForeignKey("AsuransiId")]
        public Asuransi? Asuransi { get; set; }

        public ICollection<JadwalPraktek> JadwalPraktek { get; set; }

        // Relasi ke Dokter
        [ForeignKey("DokterId")]
        public Dokter? Dokter { get; set; }

        // Relasi ke SubPoli
        [ForeignKey("SubPoliId")]
        public SubPoli? SubPoli { get; set; }
    }
}
