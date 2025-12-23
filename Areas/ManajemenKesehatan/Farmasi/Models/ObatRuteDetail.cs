using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class ObatRuteDetail : UserActivity
    {
        [Key]
        public Guid DetailRuteObatId { get; set; }
        public Guid? RuteObatId { get; set; }
        public string? NamaSingkat {  get; set; }
        public string? Kepanjangan { get; set; }
        public string? Keterangan { get; set; }
    }
}
