using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokterAsuransi", Schema = "public")]
    public class DokterAsuransi : UserActivity
    {
        [Key]
        public Guid DokterAsuransiId { get; set; }
        public Guid DokterId { get; set; }

        public string KodeDokterAsuransi { get; set; }
        public string NamaAsuransi { get; set; }

        //relasi ke Asuransi
        [ForeignKey("AsuransiId")]
        public Asuransi? Asuransi { get; set; }

        // Relasi ke Dokter
        [ForeignKey("DokterId")]
        public Dokter? Dokter { get; set; }
    }
}
