using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TindakanPerawat : UserActivity
    {
        [Key]
        public Guid? TindakanPerawatId { get; set; }
        public string? NamaTindakanPerawat {  get; set; }
        public bool? KategoriTindakan {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
