namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilSetBakteriViewModel
    {
        public Guid? LabHasilId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        public Guid? AsalSpecimenId { get; set; }

        public Guid? JenisSpecimenId { get; set; }

        public string? Keterangan { get; set; }
    }
}
