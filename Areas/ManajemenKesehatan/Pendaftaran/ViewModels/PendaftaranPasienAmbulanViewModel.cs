using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class PendaftaranPasienAmbulanViewModel
    {
        public Guid PasienId { get; set; }
        public string NoRekamMedis { get; set; }
        public string NamaPasien { get; set; }
        public string AlamatPasien { get; set; }
        public string NoTelpPasien { get; set; }
        public string JenisKelamin { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? TanggalLahir { get; set; }
        public string? Title { get; set; }

        // keterangan ambulan
        public string LayananAmbulan { get; set; }
        public string DaerahTujuan { get; set; }
        public int KelebihanJarak { get; set; }
        public int KelebihanWaktu { get; set; }
        public int JumlahParamedis { get; set; }
        public bool? IsAntarJemput { get; set; }
        public string? Catatan { get; set; }
    }
}
