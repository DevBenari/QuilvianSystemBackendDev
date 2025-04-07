using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKategoriObat", Schema = "public")]
    public class KategoriObat : UserActivity
    {
        [Key]
        public Guid KategoriObatId { get; set; }
        public string KodeKategoriObat { get; set; }
        public string CategoryExtGroupCode { get; set; }
        public string NamaKategoriObat { get; set; }
    }
}
