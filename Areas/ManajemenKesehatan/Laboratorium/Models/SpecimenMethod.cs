using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class SpecimenMethod : UserActivity
    {
        [Key]
        public Guid SpecimenMethodId { get; set; } 
        public string? CaraPengambilanSpecimen { get; set; } 
        public string? KodeSpecimenMethod { get; set; } 
        public Guid? SpecimenId { get; set; }
        public string? Keterangan { get; set; }
    }
}
