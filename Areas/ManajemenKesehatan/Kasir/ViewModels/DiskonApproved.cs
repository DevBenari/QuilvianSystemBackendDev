using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class DiskonApproved : UserActivity
    {
        [Key]
        public Guid DiskonAprrovedId { get; set; }
        public Guid? DiskonId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }

        // Approval Tahap 1
        public Guid? Approved1Id { get; set; } // User Id yang approved pertama
        public bool? IsApproved1 { get; set; } = false;
        public DateTime? ApprovedDate1 { get; set; }

        // Approval Tahap 2
        public Guid? Approved2Id { get; set; } // User Id yang approved kedua
        public bool? IsApproved2 { get; set; } = false;
        public DateTime? ApprovedDate2 { get; set; }

        // Approval Tahap 3
        public Guid? Approved3Id { get; set; } // User Id yang approved ketiga
        public bool? IsApproved3 { get; set; } = false;
        public DateTime? ApprovedDate3 { get; set; }
    }
}
