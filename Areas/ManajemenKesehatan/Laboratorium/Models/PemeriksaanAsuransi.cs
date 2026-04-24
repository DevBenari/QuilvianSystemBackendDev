using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class PemeriksaanAsuransi : UserActivity
    {
        [Key]
        public Guid PemeriksaanAsuransiId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? Keterangan {  get; set; }
    }
}
