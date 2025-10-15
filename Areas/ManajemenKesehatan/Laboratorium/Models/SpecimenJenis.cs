using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class SpecimenJenis : UserActivity
    {
        [Key]
        public Guid JenisSpecimenId { get; set; } 
        public string? NamaJenisSpecimen { get; set; } 
        public string? KodeJenisSpecimen { get; set; } 
        public Guid? AsalSpecimenId { get; set; }
        public string? Keterangan { get; set; }
    }
}
