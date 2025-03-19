using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    public class PendaftaranPasienOptik : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienOptikId { get; set; }
        public string KodePasienOptik { get; set; }
        public string NamaPasien { get; set; }
        public string? Title { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime? TTL { get; set; }
        public string JenisKelamin { get; set; } // radio button
        public string NoTelp { get; set; }
        public string? Alamat { get; set; }
        public string DokterOptik { get; set; } //selectiom
        //public string Tindakan { get; set; }

    }
}
