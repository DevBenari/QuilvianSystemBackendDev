using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("MstSpecimenAsal", Schema = "public")]

    public class SpecimenAsal : UserActivity
    {
        [Key]
        public Guid SpecimenAsalId { get; set; }
        public string? AsalSpecimen { get; set; } 
        public string? KodeAsalSpecimen { get; set; }
        public string? Keterangan { get; set; }
    }
}
