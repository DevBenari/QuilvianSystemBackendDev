using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models
{
    public class LaporanAnestesiDetail : UserActivity
    {
        [Key]
        public Guid DetailLaporanAnestesiId { get; set; } // Generate Otomatis
        public Guid? LaporanAnestesiId { get; set; }
        public decimal? VMSevoflurane { get; set; }
        public decimal? TotalSevoflurane { get; set; }
        public decimal? VMIsoflurane { get; set; }
        public decimal? TotalIsoflurane { get; set; }
        public decimal? VMEnflurane { get; set; }
        public decimal? TotalEnflurane { get; set; }
        public string? FlowO2 { get; set; }
        public string? FlowN2O { get; set; }
        public string? GolonganDarah { get; set; }
        public string? TransfusiSebelumnya { get; set; }
        public decimal? Cairan { get; set; }
        public decimal? Kristaloid { get; set; }
        public decimal? Koloid { get; set; }
        public string? KeadaanPernapasan { get; set; }
        public string? StatusGizi { get; set; }
        public string? ASA { get; set; }
        public decimal? Pendarahan { get; set; }
        public string? Keterangan { get; set; }
    }
}

