using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class AssesmentEdukasiDetail : UserActivity
    {
        [Key]
        public Guid DetailAsesmenEdukasiId { get; set; }

        public Guid? AsesmenEdukasiId { get; set; }
        public string? TopikEdukasi { get; set; }
        public DateTime? TglDetailAsesmenEdukasi { get; set; }
        public decimal? DurasiWaktu { get; set; }
        public Guid? TTDWaliId { get; set; }
        public string? NamaWali { get; set; }
        public string? TTDWaliPath { get; set; }
        public string? TingkatPemahaman { get; set; }
        public string? MetodeEdukasi { get; set; }
        public string? SaranaEdukasi { get; set; }
        public Guid? TTDPerawatId { get; set; }
        public string? TTDPerawatPath { get; set; }
        public string? EvaluasiEdukasi { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglEvaluasiEdukasi { get; set; }
    }
}
