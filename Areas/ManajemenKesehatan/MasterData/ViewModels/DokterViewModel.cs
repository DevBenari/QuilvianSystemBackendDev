using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokterViewModel
    {
        public string NmDokter { get; set; }
        public string Sip { get; set; }
        public string Str { get; set; }
        public string? TglSip { get; set; }
        public string? TglStr { get; set; }
        public string Nik { get; set; }
        public string Email { get; set; }
        public string Nohp { get; set; }
        public string Alamat { get; set; }
        public bool? IsAsuransi { get; set; }

        // Informasi Tambahan
        public IFormFile? Foto { get; set; }
        public string? FotoName { get; set; }
        public string? FotoPath { get; set; }

        public List<Guid>? AsuransiId { get; set; }
        public List<Guid>? PoliId { get; set; }

    }
}
