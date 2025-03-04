using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokterViewModel
    {
        public string NmDokter { get; set; }
        public string Sip { get; set; }
        public string Str { get; set; }
        public DateTime? TglSip { get; set; }
        public DateTime? TglStr { get; set; }
        public string Nik { get; set; }
        public string Email { get; set; }
        public string Nohp { get; set; }
        public string Alamat { get; set; }
        public bool? IsAsuransi { get; set; }

        // Informasi Tambahan
        public string? FotoDokter { get; set; }
        public string? JudulFileFoto { get; set; }
        public string? FotoPath { get; set; }
        public string? FotoByte { get; set; }
    }
}
