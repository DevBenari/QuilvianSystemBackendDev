using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    public class DiskonDokter: UserActivity
    {
        [Key]
        public Guid DiskonApprovedId {  get; set; }
        public Guid? DiskonId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? Approved1Id { get; set; }
        public bool? IsApproved1 { get; set; }
        public DateTime? ApprovedDate1 { get; set; }
    }
}
