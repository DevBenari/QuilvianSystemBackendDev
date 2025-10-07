using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("MstSpecimen", Schema = "public")]

    public class Specimen : UserActivity
    {
        [Key]
        public Guid SpecimenId { get; set; }
        public string? NamaSpecimen { get; set; } 
        public string? KodeSpecimen { get; set; }
        public string? Keterangan { get; set; }
    }
}
