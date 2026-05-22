namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class KunjunganLayananViewModel
    {
        public Guid? KunjunganId { get; set; }

        public Guid? InstalasiUnitId { get; set; }

        public Guid? PoliklinikId { get; set; }

        public Guid? DokterId { get; set; }

        public string? JenisLayanan { get; set; }
        // contoh: RAJAL, RANAP, IGD, ICU, OK

        public DateTime? TglMasukLayanan { get; set; }

        public DateTime? TglKeluarLayanan { get; set; }
    }
}
