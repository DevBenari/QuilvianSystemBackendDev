using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    [Table("BiayaAdministrasi", Schema = "public")]
    public class BiayaAdministrasi : UserActivity
    {
        [Key]
        public Guid BiayaAdministrasiId { get; set; }
        public string? BiayaAdministrasiKode { get; set; }
        public string? NamaBiayaAdministrasi { get; set; }
        public decimal? NominalBiayaAdministrasi { get; set; }
        public bool? IsDelete { get; set; }
    }
}
