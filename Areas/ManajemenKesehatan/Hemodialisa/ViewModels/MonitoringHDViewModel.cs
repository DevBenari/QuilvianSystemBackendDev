namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.ViewModels
{
    public class MonitoringHDViewModel
    {
        public Guid? HasilHemodialisaId { get; set; }

        public decimal? NoDx { get; set; }

        public TimeOnly? JamMonitoring { get; set; }

        public string? Tensi { get; set; }
        public decimal? Nadi { get; set; }
        public decimal? TD { get; set; }
        public decimal? VP { get; set; }
        public decimal? AP { get; set; }
        public decimal? QB { get; set; }
        public decimal? QD { get; set; }
        public decimal? TMP { get; set; }
        public decimal? DP { get; set; }

        public decimal? UF { get; set; }

        public string? Keluhan { get; set; }
        public string? Terapi { get; set; }
        public string? Keterangan { get; set; }
        
    }
}
