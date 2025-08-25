using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class DetailKetergantungan :UserActivity
    {
        [Key]
        public Guid DetKetergantunganId { get; set; }   
        public Guid? KunjunganId { get; set; }           
        public Guid? PengkajianPerawatId { get; set; }   
        public Guid? KetergantunganId { get; set; }
        public Guid[]? IndikatorPengkajianIds { get; set; }
        public string? Keterangan { get; set; }
    }
}
