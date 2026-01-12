using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class HandoverPasienDetail : UserActivity
    {
        [Key]
        public Guid DetailHandoverPasienId {  get; set; }
        public Guid? HandoverPasienId { get; set; }
        public Guid? ChecklistItemId { get; set; }
        public bool? IsSudah {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
