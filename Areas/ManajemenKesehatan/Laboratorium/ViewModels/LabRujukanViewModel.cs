namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabRujukanViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? LabId { get; set; }
        public string? ArahRujukan { get; set; }
        public Guid? FaskesRujukanId { get; set; }
        public string? DokterPerujuk { get; set; }
        public string? TglRujukan { get; set; }
        public string? Keterangan { get; set; }
    }
}
