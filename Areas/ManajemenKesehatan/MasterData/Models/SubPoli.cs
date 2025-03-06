using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstSubPoli", Schema = "public")]
    public class SubPoli : UserActivity
    {

        [Key]
        public Guid SubPoliId { get; set; } 
        public Guid PoliId { get; set; }
        public string NamaSubPoli { get; set; }
        public string KodeSubPoli { get; set; }
        public string? Deskripsi { get; set; }
        public string KepalaSubPoli { get; set; }
        public string Lokasi { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string HariOperasional { get; set; }
        public DateTime? JamBuka { get; set; }
        public DateTime? JamTutup { get; set; }
        public string? LayananSubPoli { get; set; }
        public int JumlahMaxPasien { get; set; }

        // Relasi ke Poliklinik
        [ForeignKey("PoliId")]
        public Poliklinik Poliklinik { get; set; }
    }
}
