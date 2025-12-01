namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDTindakanDetailViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? TindakanId { get; set; }
        public string? HasilSkinTest { get; set; }
        public string? HasilTetanusToxoid { get; set; }
        public string? HasilMedikamentosa { get; set; }
        public decimal? JumlahAntiTetanusSerum { get; set; }
        public string? JalurMedikamentosa { get; set; }
        public TimeOnly? WaktuPengobatan { get; set; }
        public Guid? PerawatId { get; set; }
        public Guid? DokterId { get; set; }
        public string? Keterangan { get; set; }
        public string? KategoriTindakan { get; set; }
        public DateTime? WaktuTindakan { get; set; }

    }
}
