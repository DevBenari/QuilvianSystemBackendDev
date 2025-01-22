using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasien", Schema = "dbo")]
    public class PendaftaranPasien : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienId { get; set; }

        public string NoRekamMedis { get; set; }
        public string NamaLengkap { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport

        // Tempat dan Tanggal Lahir
        public string TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }

        // Informasi Penjamin
        public string Penjamin { get; set; }
        public string Layanan { get; set; }
        public string DokterPemeriksa { get; set; }

    }

}
