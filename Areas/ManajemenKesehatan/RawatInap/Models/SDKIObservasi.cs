using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class SDKIObservasi : UserActivity
    {
        [Key]
        public Guid SDKIObservasiId { get; set; }
        public Guid? SDKIDiagnosaId { get; set; }
        public string? NamaObservasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
