using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class ObatTelaah : UserActivity
    {
        [Key]
        public Guid TelaahObatId { get; set; }          // Generate otomatis
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid ResepId { get; set; }
        public bool IsTepatIdentitas { get; set; }
        public bool IsTepatObat { get; set; }
        public bool IsTepatDosis { get; set; }
        public bool IsTepatRute { get; set; }
        public bool IsTepatWaktu { get; set; }

        public Guid? PetugasCekFinalId { get; set; }
        public string? TTDPetugasCekFinal { get; set; }  // Path atau URL
        public string? Keterangan { get; set; }
    }
}
