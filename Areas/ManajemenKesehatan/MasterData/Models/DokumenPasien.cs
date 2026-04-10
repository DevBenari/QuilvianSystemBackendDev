using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class DokumenPasien : UserActivity
    {
        [Key]
        public Guid DokumenPasienId { get; set; }
        public Guid? PasienId { get; set; }
        public string? JenisDokumen {  get; set; }
        public string? PathDokumen { get; set; }
        public string? Keterangan {  get; set; }
    }
}
