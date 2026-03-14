namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PaketLayananVM
    {
        public string? KodePaketLayanan { get; set; }
        public string? NamaPaketLayanan { get; set; }
        public DateTime? TglPembuatan { get; set; }
        public Guid? LayananId { get; set; }
        public string? Keterangan { get; set; }

        public List<PaketLayananDetailVM>? Details { get; set; }

    }
}
