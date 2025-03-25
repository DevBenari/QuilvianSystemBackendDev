using System.Text.Json.Serialization;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class RegistFasilitasPasienViewModel
    {
        public Guid PasienId { get; set; }
        public string NamaPasien { get; set; }
        public string NoRekamMedis { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TTL { get; set; }
        public string JenisKelamin { get; set; }
        public string Alamat { get; set; }
        public string NoTelepon { get; set; }
        public string DokterPemeriksa { get; set; }
        public string NamaFasilitasPasien { get; set; }

    }
}
