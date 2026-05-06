namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class NosokomialViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }

        public decimal? TB { get; set; }
        public decimal? BB { get; set; }

        public string? CaraMasukRS { get; set; }
        public DateTime? TglMasukRs { get; set; }
        public DateTime? TglKeluarRs { get; set; }

        public Guid? DokterId1 { get; set; }
        public Guid? DokterId2 { get; set; }
        public Guid? DokterId3 { get; set; }

        public Guid? IPCLN1 { get; set; }
        public Guid? IPCLN2 { get; set; }
        public Guid? IPCLN3 { get; set; }

        public string? KondisiKeluar { get; set; }
        public string? DiagnosaAwal { get; set; }
        public string? DiagnosaAkhir { get; set; }
        public Guid? PerawatId { get; set; }
        public bool? Status { get; set; }
    }
}
