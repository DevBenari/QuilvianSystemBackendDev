using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPPN", Schema = "public")]
    public class PPN : UserActivity
    {
        [Key]
        public Guid PpnId { get; set; }
        public string? NamaPpn { get; set; } = default!;  
        public decimal? Persentase { get; set; }      
        public bool? IsAktif { get; set; }                  
        public string? Keterangan { get; set; }             
    }
}
