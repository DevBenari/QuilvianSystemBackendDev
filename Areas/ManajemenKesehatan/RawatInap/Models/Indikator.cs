using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class Indikator : UserActivity
    {
        [Key]
        public Guid? IndikatorId { get; set; } 
        public string? NamaIndikator { get; set; }
        public string? Keterangan { get; set; } 

    }

}
