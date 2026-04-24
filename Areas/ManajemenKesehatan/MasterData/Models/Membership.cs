using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstMembership", Schema = "public")]
    public class Membership : UserActivity
    {
        [Key]
        public Guid MembershipId { get; set; }
        public string? NamaMembership { get; set; }
        public string? Keterangan { get; set; }
        public decimal? BiayaMembership { get; set; }
        public bool? IsAktif { get; set; } = true;
        public string? Durasi { get; set; }
    }
}
