using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasienUGD", Schema = "public")]
    public class PendaftaranPasienUGD : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienUGDId { get; set; }
        public string KodePasienUGD { get; set; }
        public string NamaPasien { get; set; }
        public string? Title { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TTL { get; set; }
        public int? Umur { get; set; }   // perhitungan dari tanggal lahir
        public string? NoTelp { get; set; }

        // informasi nakes yang menangani
        public string? NamaDokterUGD { get; set; }
        public string? Diagnosa { get; set; }
        public string? Tindakan { get; set; }
        

        // biaya
        public decimal? BiayaAdmin { get; set; }
        public string? Kelas { get; set; }

        // relasi ke asuransi pasien
        public string? AsuransiId { get; set; }
        public string? NoPolis { get; set; }
        public string? NamaAsuransi { get; set; }
        public string? Afliasi { get; set; }


    }
}
