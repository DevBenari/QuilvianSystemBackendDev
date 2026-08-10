using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasilSpecimenJenis
    {
        [Key]
        public Guid LabHasilSpecimenJenisId { get; set; }

        public Guid? LabHasilSpecimenId { get; set; }

        public Guid? JenisSpecimenId { get; set; }

        // navigation
        public LabHasilSpecimen? LabHasilSpecimen { get; set; } = null!;

        public SpecimenJenis? JenisSpecimen { get; set; } = null!;
    }
}
