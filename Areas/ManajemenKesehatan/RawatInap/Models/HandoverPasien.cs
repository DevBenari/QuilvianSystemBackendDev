using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class HandoverPasien : UserActivity
    {
        [Key]
        public Guid HandoverPasienId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalSerahTerima {  get; set; }
        public Guid? AdministrationId { get; set; }
        public Guid? CROId { get; set; }
        public Guid? PerawatId { get; set; }
        public string? PathTTDAdministration {  get; set; }
        public string? PathTTDCRO {  get; set; }
        public string? PathTTDPerawat {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
