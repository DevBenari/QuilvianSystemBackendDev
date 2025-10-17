using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class EvaluasiAwalDetail : UserActivity  
    {
        [Key]
        public Guid DetailEvaluasiAwalId { get; set; }   // Generate Otomatis
        public Guid? EvaluasiAwalId { get; set; }         // Relasi dengan tabel EvaluasiAwal
        public Guid? ChecklistItemId { get; set; }        // Relasi dengan tabel ChecklistItem
        public string? Keterangan { get; set; }
        public DateTime? TglPenyimpanan { get; set; }
    }
}
