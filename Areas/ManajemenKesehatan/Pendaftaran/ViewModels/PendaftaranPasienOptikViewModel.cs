using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class PendaftaranPasienOptikViewModel
    {
        public string NamaPasien { get; set; }
        public string? Title { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TTL { get; set; }
        public string JenisKelamin { get; set; } // radio button
        public string NoTelp { get; set; }
        public string? Alamat { get; set; }
        public string DokterOptik { get; set; } //selectiom
    }
}
