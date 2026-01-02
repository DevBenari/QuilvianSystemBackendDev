using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class InformasiPenundaan : UserActivity
    {
        [Key]
        public Guid InfoPenundaanId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalInfoTunda { get; set; }
        public Guid? Keterangan {  get; set; }
        public Guid? PerawatId { get; set; }
    }
}
