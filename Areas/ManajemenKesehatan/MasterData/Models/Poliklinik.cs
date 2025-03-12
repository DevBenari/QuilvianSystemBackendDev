using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    [Table("MstPoliklinik", Schema = "public")]
    public class Poliklinik : UserActivity
    {
        [Key]
        public Guid PoliklinikId { get; set; }
        public string KodePoliklinik { get; set; }
        public string NamaPoliklinik { get; set; }
        public string KepalaPoliklinik { get; set; }
        public string Lokasi { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string HariOperasional {get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamBuka { get; set; }

        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly? JamTutup { get; set; }
        public string? LayananPoliklinik { get; set; }
        public string? Deskripsi { get; set; }
        public int JumlahMaxPasien { get; set; }

        // Relasi One-to-Many ke SubPoli
        public ICollection<SubPoli> SubPolis { get; set; }
    }
}
