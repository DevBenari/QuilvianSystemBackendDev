using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKategoriIndikator", Schema ="public")]
    public class KategoriIndikator : UserActivity
    {
        [Key]
        public Guid KategoriIndikatorId { get; set; }
        public string? NamaKategoriIndikator {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
