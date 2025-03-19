using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("RgsFasilitasPasien", Schema = "public")]
    public class RegistFasilitasPasien : UserActivity
    {
        [Key]
        public Guid RegistFasilitasPasienId { get; set; }
        public Guid PasienId { get; set; }
        public string KodeRegistFasilitas { get; set; }
        public string NamaPasien { get; set; }
        public string NoRekamMedis { get; set; }
        public DateTime? TTL { get; set; }
        public string JenisKelamin { get; set; }
        public string Alamat { get; set; }
        public string NoTelepon { get; set; }
        public string DokterPemeriksa { get; set; }
        public string NamaFasilitasPasien { get; set; }
    }
}
