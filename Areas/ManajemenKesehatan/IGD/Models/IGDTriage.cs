using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class IGDTriage : UserActivity
    {
        [Key]
        public Guid? TriageId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string? KeluhanUtama {  get; set; }
        public string? DiteruskanKepada { get; set; }
        public DateTime? WaktuMasuk {  get; set; }
        public string? DikirimKe {  get; set; }
        public bool? Status { get; set; }
        public string? Keterangan {  get; set; }
    }
}
