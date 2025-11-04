using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class IGDTindakanDetail : UserActivity
    {
        [Key]
        public Guid DetailTindakanIGDId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? TindakanId {  get; set; }
        public string? KategoriTindakan {  get; set; }
        public DateTime? WaktuTindakan { get; set; }
        public string? TTDPath { get; set; }
        public string? Keterangan {  get; set; }
    }
}
