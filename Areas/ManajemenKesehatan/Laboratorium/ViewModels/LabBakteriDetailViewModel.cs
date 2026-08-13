namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabBakteriDetailViewModel
    {
        public Guid? LabHasilBakteriId { get; set; }

        public Guid? KunjunganId { get; set; }

        public Guid? PasienId { get; set; }

        public Guid AntibiotikId { get; set; }

        public string? RangeZona { get; set; }

        public decimal? ZonaMM { get; set; }

        public string? ResultAntibiotik { get; set; }

        public string? Keterangan { get; set; }
    }
}
