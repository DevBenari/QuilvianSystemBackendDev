using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;
namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    [Table("MstOperasi", Schema = "public")]
    public class Operasi
    {
        // informasi umum
        [Key]
        public Guid OperasiId { get; set; }
        public string KodeOperasi { get; set; }
        public string JenisOperasi { get; set; }
        public string TipeOperasi { get; set; }
        public string NamaTindakanOperasi { get; set; }
        public DateTime TanggalOperasi { get; set; }
        public string StatusOperasi { get; set; }
        public int LamaOperasi { get; set; }
        public string RuanganOperasi { get; set; }
        public string LokasiRuanganOperasi { get; set; }
        public bool TipeCCVC { get; set; }
        public string? CatatanMedis { get; set; }

        // informasi nakess


    }
}
