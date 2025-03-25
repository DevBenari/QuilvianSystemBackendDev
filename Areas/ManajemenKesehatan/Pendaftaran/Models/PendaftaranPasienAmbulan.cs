using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasienAmbulan", Schema = "public")]
    public class PendaftaranPasienAmbulan : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienAmbulanId { get; set; }
        public Guid PasienId { get; set; }
        public string KodePdfPasienAmbulan { get; set; }
        public string NoRekamMedis { get; set; }
        public string NamaPasien { get; set; }
        public string? AlamatPasien { get; set; }
        public string? NoTelpPasien { get; set; }
        public string? JenisKelamin { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TanggalLahir { get; set; }
        public string? Title { get; set; }

        // keterangan ambulan
        public string? LayananAmbulan { get; set; }
        public string? DaerahTujuan { get; set; }
        public int? KelebihanJarak { get; set; }
        public int? KelebihanWaktu { get; set; }
        public int? JumlahParamedis { get; set; }
        public bool? IsAntarJemput { get; set; }
        public string? Catatan { get; set; }
    }
}
