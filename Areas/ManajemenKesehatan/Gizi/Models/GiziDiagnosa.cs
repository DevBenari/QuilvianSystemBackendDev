using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Models
{
    public class GiziDiagnosa : UserActivity
    {
        [Key]
        public Guid GiziDiagnosaId { get; set; }
        public string? KodeDiagnosa { get; set; }
        public string? DiagnosaGizi { get; set; }
        public string? HasilDiagnosa { get; set; }
        public string? GroupDiagnosa { get; set; }
        public string? Keterangan {  get; set; }
    }
}
