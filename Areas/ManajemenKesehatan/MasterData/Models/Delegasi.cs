using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("Delegasi", Schema = "public")]
    public class Delegasi : UserActivity
    {
        [Key]
        public Guid DelegasiId { get; set; }
        public bool? IsDelegated { get; set; } // Menandakan apakah ada delegasi
        public Guid? UserDelegasiId { get; set; } // ID pengguna yang didelegasikan
        public Guid? UserActiveId { get; set; } // ID pengguna yang aktif
        public string? Tugas { get; set; } // Deskripsi tugas yang didelegasikan
    }
}
