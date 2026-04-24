namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class PemeriksaanAsuransiViewModel
    {
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? Keterangan { get; set; }
    }
}
