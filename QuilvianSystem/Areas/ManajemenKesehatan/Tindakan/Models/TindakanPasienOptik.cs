using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienOptik", Schema = "dbo")]
    public class TindakanPasienOptik : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienId { get; set; }

        // Informasi Registrasi
        public string NoRegistrasi { get; set; }
        public string NoRekamMedis { get; set; }

        // Dokter Pemeriksa
        public string DokterPemeriksa { get; set; } // Selection dokter pemeriksa

        // Tindakan Layanan
        public string TindakanLayanan { get; set; } // Selection tindakan layanan

        // Registrasi Optik (tindakan dan jumlah)
        public string Tindakan { get; set; } // Tindakan yang dilakukan
        public int Jumlah { get; set; } // Jumlah tindakan yang dilakukan
    }

}
