namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels
{
    public class RecallDetailViewModel
    {
        public Guid RecallId { get; set; }
        public string? MakananSelingan { get; set; }
        public string? WaktuMakanan { get; set; }
        public decimal? BanyakGR { get; set; }
        public decimal? BanyakUTR { get; set; }
        public bool? IsSelingan { get; set; }
        public decimal? KAL { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Lemak { get; set; }
        public decimal? CHO { get; set; }
        public decimal? CA { get; set; }
        public decimal? FE { get; set; }
        public decimal? VitA { get; set; }
        public decimal? VitB1 { get; set; }
        public decimal? VitC { get; set; }
        public bool? IsRataRataHarian { get; set; }
        public bool? IsRDA { get; set; }
        public string? Keterangan { get; set; }
    }
}
