using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class OperasiViewModel
    {
        public string JenisOperasi { get; set; }
        public string TipeOperasi { get; set; }
        public string NamaTindakanOperasi { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly TanggalOperasi { get; set; }
        public string StatusOperasi { get; set; }
        public int LamaOperasi { get; set; }
        public string RuanganOperasi { get; set; }
        public string LokasiRuanganOperasi { get; set; }
        public bool TipeCCVC { get; set; }
        public string? CatatanMedis { get; set; }

        // informasi nakess
        public string NamaDokterOperator { get; set; }
        public string NamaDokterAnastesi { get; set; }
        public string? DokterTambahan1 { get; set; }
        public string? DokterTambahan2 { get; set; }
        public string? DokterTambahan3 { get; set; }
        public string? DokterTambahan4 { get; set; }
        public string? DokterTambahan5 { get; set; }

        // informasi pasien
        public Guid PasienId { get; set; }
        public string NamaPasien { get; set; }
        public string KeluhanOperasi { get; set; }
    }
}
