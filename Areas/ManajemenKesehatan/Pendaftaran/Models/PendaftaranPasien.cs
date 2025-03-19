using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasien", Schema = "public")]
    public class PendaftaranPasien : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienId { get; set; }

        public string NoRekamMedis { get; set; }
        public string NamaLengkap { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport

        // Tempat dan Tanggal Lahir
        public string TempatLahir { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TanggalLahir { get; set; }

        // Informasi Penjamin
        public string Penjamin { get; set; }
        public string Layanan { get; set; }
        public string DokterPemeriksa { get; set; }

    }

}
