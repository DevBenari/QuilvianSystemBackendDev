using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class SpecimenPemeriksaan : UserActivity
    {
        [Key]
        public Guid SpecimenPemeriksaanId { get; set; }
        public string? PemeriksaanSpecimen { get; set; } 
        public string? KodeSpecimenTest { get; set; }
        public Guid? JenisSpecimenId { get; set; }
        public string? Keterangan { get; set; }
    }
}
