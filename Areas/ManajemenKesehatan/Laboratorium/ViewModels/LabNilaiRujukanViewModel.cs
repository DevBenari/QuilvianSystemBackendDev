namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabNilaiRujukanViewModel
    {
        public Guid? PemeriksaanLabId { get; set; }
        public string? JenisKelamin { get; set; }
        public DateOnly? DariUmur { get; set; }
        public DateOnly? SampaiUmur { get; set; }
        public decimal? NilaiMinimum { get; set; }
        public decimal? NilaiMaximum { get; set; }
        public string? NilaiNormal { get; set; }
        public string? HasilNilaiNormal { get; set; }
        public string? StatusNilaiNormal { get; set; }
        public string? Keterangan { get; set; }
    }
}
