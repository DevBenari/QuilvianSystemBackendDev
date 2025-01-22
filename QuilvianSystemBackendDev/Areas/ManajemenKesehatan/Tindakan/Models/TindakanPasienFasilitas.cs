using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienFasilitas", Schema = "dbo")]
    public class TindakanPasienFasilitas : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienFasilitasId { get; set; }

        // Informasi Registrasi
        public string NoRegistrasi { get; set; }
        public string NoRekamMedis { get; set; }

        // Dokter Pemeriksa
        public string DokterPemeriksa { get; set; } // Selection list dokter pemeriksa

        // Fasilitas yang disediakan
        public string Fasilitas { get; set; } // nanti berelasi dengan fasilitas

    }

}
