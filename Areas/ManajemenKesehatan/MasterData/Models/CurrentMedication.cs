using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstCurrentMedication", Schema = "public")]
    public class CurrentMedication : UserActivity
    {
        [Key]
        public Guid CurrentMedicationID { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PendaftaranPasienBaruId { get; set; }
        public string? NoRekamMedis { get; set; }
        public Guid? ObatId { get; set; }
        public string? NamaObat { get; set; }
        public string? Dosis { get; set; }
        public string? Frekuensi { get; set; }
        public string? LamaKonsumsi { get; set; }
        public string? Status { get; set; }
        public string? Keterangan { get; set; }
    }
}
