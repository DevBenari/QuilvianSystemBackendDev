using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstRacikanAddon", Schema = "public")]
    public class RacikanAddon : UserActivity
    {
        [Key]
        public Guid AddonRacikanId { get; set; }
        public Guid? BentukSatuanId { get; set; }
        public string? NamaBentukSatuan { get; set; }
        public decimal? BiayaJasaRacikan { get; set; }
        public decimal? BiayaKemasanRacikan { get; set; }
        public string? Keterangan { get; set; }
    }
}
