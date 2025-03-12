using Newtonsoft.Json;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokter", Schema = "public")]
    public class Dokter : UserActivity
    {
        [Key]
        public Guid DokterId { get; set; }
        public string KdDokter { get; set; }
        public string NmDokter { get; set; }
        public string Sip { get; set; }
        public string Str { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TglSip { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TglStr { get; set; }
        public string Nik { get; set; }
        public string Email { get; set; }
        public string Nohp { get; set; }
        public string Alamat { get; set; }
        public bool? IsAsuransi { get; set; }

        public string? FotoName { get; set; }
        public string? FotoPath { get; set; }

        //relasi ke dokter poli
        public ICollection<DokterPoli> DokterPolis { get; set; }

    }
}
